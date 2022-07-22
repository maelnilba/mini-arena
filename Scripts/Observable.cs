using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observable<T> 
{
    private T value;

    public Observable()
    {
    }

    public Observable(T value)
    {
        this.value = value;
    }

    public Action<Observable<T>, T, T> OnChanged;

    public T Value
    {
        get { return value; }
        set
        {
            var oldValue = this.value;
            this.value = value;
            if (OnChanged != null)
            {
                OnChanged(this, oldValue, value);
            }
        }
    }

	public static implicit operator Observable<T>(T observable)
	{
		return new Observable<T>(observable);
	}

	public static explicit operator T(Observable<T> observable)
	{
		return observable.value;
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public bool Equals(Observable<T> other)
	{
		return other.value.Equals(value);
	}

	public override bool Equals(object other)
	{
		return other != null
			&& other is Observable<T>
			&& ((Observable<T>)other).value.Equals(value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}
}
