using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T _objPrefab;
    private int _initSize = 10;

    private Transform m_Parent;

    private Queue<T> _pool = new Queue<T>();

    public event Action<T> ObjectReturnedEvent;

    public ObjectPool(T objPrefab, Transform parent = null, int initSize = 10)
    {
        _objPrefab = objPrefab;

        _initSize = initSize;

        m_Parent = parent;

        for (int i = 0; i < _initSize; i++)
        {
            T obj = GameObject.Instantiate(_objPrefab, m_Parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T GetObject()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T obj = GameObject.Instantiate(_objPrefab, m_Parent);
            return obj;
        }
    }

    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
        ObjectReturnedEvent?.Invoke(obj);
    }
}
