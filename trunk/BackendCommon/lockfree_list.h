/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#ifndef LOCKFREE_LIST
#define LOCKFREE_LIST
#pragma once

#ifdef WIN32
#	include <windows.h>
#endif

#include <utility>
#include <limits>

template<typename T>
T* compare_and_swap(T* volatile* destination, T* comparand, T* value)
{
#ifdef WIN32
	return static_cast<T*>(InterlockedCompareExchangePointer((void* volatile*) destination, value, comparand));
#else
#	error No implementation is available for this platform.
#endif
}

// http://www.cs.bgu.ac.il/~mpam092/wiki.files/lock-free-linked-list.pdf

template<typename KeyType, typename ValueType>
struct lockfree_list
{
	struct list_entry;

	struct successor_field
	{
		list_entry* get_pointer()
		{
			const size_t mask = ~ (size_t) 3;
			return reinterpret_cast<list_entry*>(reinterpret_cast<size_t>(field) & mask);
		}

		bool is_flagged() const
		{
			return lockfree_list::is_flagged((void*) field);
		}

		bool is_marked() const
		{
			return lockfree_list::is_marked((void*) field);
		}

		list_entry volatile* field;
	};

	struct list_entry
	{
		friend struct lockfree_list<KeyType, ValueType>;

		KeyType first;
		ValueType second;

	private:
		list_entry* backlink;
		successor_field successor;

		list_entry* get_right()
		{
			return successor.get_pointer();
		}

		bool is_marked() const
		{
			return successor.is_marked();
		}

		bool is_flagged() const
		{
			return successor.is_flagged();
		}
	};

	typedef std::pair<list_entry*, list_entry*> node_pair;
	typedef list_entry* iterator;
	typedef const list_entry* const_iterator;

	lockfree_list()
	{
		m_head = new list_entry;
		m_tail = new list_entry;

		m_head->first = std::numeric_limits<KeyType>::min();
		m_tail->first = std::numeric_limits<KeyType>::max();

		m_head->successor.field = m_tail;
	}

	iterator begin()
	{
		return m_head->get_right();
	}

	const_iterator begin() const
	{
		return m_head->get_right();
	}

	iterator end()
	{
		return m_tail;
	}

	const_iterator end() const
	{
		return m_tail;
	}

	iterator find(const KeyType& key)
	{
		node_pair result = search_from(key, m_head);
		if(result.first->first == key)
			return result.first;

		return m_tail;
	}

	iterator remove(const KeyType& key)
	{
		node_pair search_result = search_from_ex(key, m_head);
		list_entry* prev_node = search_result.first;
		list_entry* del_node = search_result.second;
		if(del_node->first != key)
			return NULL;
		
		std::pair<list_entry*, bool> flag_result = try_flag(prev_node, del_node);
		if(flag_result.first != NULL)
			help_flagged(flag_result.first, del_node);
		if(flag_result.second == false)
			return NULL; //no such key

		return del_node;
	}

	//Behaves like std::map::insert
	std::pair<iterator, bool> insert(const KeyType& key, const ValueType& value)
	{
		node_pair prev_next = search_from(key, m_head);
		list_entry* prev_node = prev_next.first;
		list_entry* next_node = prev_next.second;

		if(prev_node->first == key)
		{
			//duplicate key
			return std::make_pair(prev_node, false);
		}

		list_entry* new_node = new list_entry;
		new_node->first = key;
		new_node->second = value;

		while(true)
		{
			successor_field prev_succ = prev_node->successor;
			if(prev_succ.is_flagged())
			{
				help_flagged(prev_node, prev_succ.get_pointer());
			}
			else
			{
				new_node->successor.field = next_node;
				//attempt to insert
				list_entry* result = compare_and_swap((list_entry* volatile*) &prev_node->successor.field, next_node, new_node);
				if(result == next_node)
				{
					//success!
					return std::make_pair(new_node, true);
				}
				else
				{
					if(is_flagged(result))
					{
						//failure due to flagging, help finish deletion
						help_flagged(prev_node, result->successor.get_pointer());
					}

					while(prev_node->successor.is_marked())
					{
						//possible failure due to marking, traverse backlinks
						prev_node = prev_node->backlink;
					}
				}
			}

			node_pair search_result = search_from(key, prev_node);
			if(search_result.first->first == key)
			{
				//duplicate key
				delete new_node;
				return std::make_pair(search_result.first, false);
			}
		}
	}

private:
	list_entry* m_head;
	list_entry* m_tail;

	static bool is_flagged(void* pointer)
	{
		size_t value = reinterpret_cast<size_t>(pointer) & 0x3;
		return value == 0x01;
	}

	static bool is_marked(void* pointer)
	{
		size_t value = reinterpret_cast<size_t>(pointer) & 0x3;
		return value == 0x02;
	}

	static list_entry* make_flagged(list_entry* pointer)
	{
		const size_t mask = ~ (size_t) 3;
		size_t value = reinterpret_cast<size_t>(pointer) & mask;
		return reinterpret_cast<list_entry*>(value | 0x1);
	}

	static list_entry* make_marked(list_entry* pointer)
	{
		const size_t mask = ~ (size_t) 3;
		size_t value = reinterpret_cast<size_t>(pointer) & mask;
		return reinterpret_cast<list_entry*>(value | 0x2);
	}

	//finds two consecutive nodes such that n1.key (Comp) key < n2.key
	template<typename CompType>
	node_pair search_from(const KeyType& key, list_entry* current_node)
	{
		CompType comp;
		list_entry* next_node = current_node->get_right();
		while(comp(next_node->first, key))
		{
			//Ensure that either next_node is unmarked, or both curr_node and
			//next_node are marked and curr_node was marked earlier
			while(next_node->is_marked() && 
				(!is_marked(current_node) || current_node->get_right() != next_node))
			{
				if(current_node->get_right() == next_node)
					help_marked(current_node, next_node);

				next_node = current_node->get_right();
			}

			if(comp(next_node->first, key))
			{
				current_node = next_node;
				next_node = current_node->get_right();
			}
		}

		return std::make_pair(current_node, next_node);
	}

	node_pair search_from(const KeyType& key, list_entry* current_node)
	{
		return search_from<std::less_equal<KeyType>>(key, current_node);
	}

	node_pair search_from_ex(const KeyType& key, list_entry* current_node)
	{
		return search_from<std::less<KeyType>>(key, current_node);
	}

	//Attempts to mark del_node
	void try_mark(list_entry* del_node)
	{
		do
		{
			list_entry* next_node = del_node->get_right();
			list_entry* result = compare_and_swap((list_entry* volatile*) &del_node->successor.field, next_node, make_marked(next_node));
			if(is_flagged(result))
			{
				//failed due to flagging
				help_flagged(del_node, result->get_right());
			}
		} while(!del_node->is_marked());
	}

	std::pair<list_entry*, bool> try_flag(list_entry* prev_node, list_entry* target_node)
	{
		while(true)
		{
			list_entry* flagged_target = make_flagged(target_node);
			if(prev_node->successor.field == flagged_target)
			{
				//predecessor is already flagged, report failure
				return std::make_pair(prev_node, false);
			}

			//attempt to flag
			list_entry* result = compare_and_swap((list_entry* volatile*) &prev_node->successor.field, target_node, flagged_target);
			if(result == target_node)
			{
				//flag succeeded
				return std::make_pair(prev_node, true);
			}
			if(result == flagged_target)
			{
				//failed due to concurrent flagging operation
				return std::make_pair(prev_node, false);
			}

			while(prev_node->is_marked())
			{
				//Possibly a failure due to marking. Traverse a chain of backlinks for an unmarked node.
				prev_node = prev_node->backlink;
			}

			node_pair prev_del = search_from_ex(target_node->first, prev_node);
			if(prev_del.second != target_node)
			{
				//target node got deleted, return failure without a pointer
				return std::make_pair<list_entry*, bool>(NULL, false);
			}
		}

		//this should be unreachable
		throw;
	}

	//Attempts to physically delete the marked node del_node and unflag prev_node
	void help_marked(list_entry* prev_node, list_entry* del_node)
	{
		list_entry* next_node = del_node->get_right();
		//TODO: make_flagged here appears to disagree with what the function description says
		compare_and_swap((list_entry* volatile*) &prev_node->successor.field, make_flagged(del_node), next_node);
		//TODO: where should the node actually be freed?
	}

	//Attempts to mark and physically delete del_node, which is the successor of the flagged node prev_node
	void help_flagged(list_entry* prev_node, list_entry* del_node)
	{
		del_node->backlink = prev_node;
		if(!del_node->is_marked())
			try_mark(del_node);

		help_marked(prev_node, del_node);
	}
};

#endif
