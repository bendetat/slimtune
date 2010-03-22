#ifndef __SLIMCOMPTR__
#define __SLIMCOMPTR__

template<typename T> class SlimComPtr
{
	T* data;

public:
	SlimComPtr()
	{
		data = NULL;
	}

	~SlimComPtr()
	{
		if(data != NULL)
		{
			data->Release();
			data = NULL;
		}
	}

	inline T* operator->()
	{
		return data;
	}

	inline T** operator&()
	{
		return &data;
	}

	inline operator T*()
	{
		return data;
	}
};

#endif