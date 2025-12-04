using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    private static List<IUpdateObserver> _observers = new List<IUpdateObserver>();
    private static List<IUpdateObserver> _pendingObservers = new List<IUpdateObserver>();
    private static List<IUpdateObserver> _observersToRemove = new List<IUpdateObserver>();    

    private static int _currentIndex;

    public static UpdateManager _instance;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        //Lets process the pending removals first
        for(int i = 0; i < _observersToRemove.Count; i++)
        {
            int actualIndex = _observers.IndexOf(_observersToRemove[i]);
            _observers.Remove(_observersToRemove[i]);
            Debug.Log($"Removed the Update Method {_observersToRemove[i]} at index {actualIndex} (loop i = {i})");
        }
        _observersToRemove.Clear();

        for(_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex-- )
        {
            _observers[_currentIndex]?.ObservedUpdate();
        }

        _observers.AddRange(_pendingObservers);
        _pendingObservers.Clear();
    }


    public static void RegisterObserver(IUpdateObserver observer)
    {
        if(!_observers.Contains(observer) && !_pendingObservers.Contains(observer))
        {
            _pendingObservers.Add(observer);
            Debug.Log($"Added the Update Method {observer}");
        }    
    }

    public static void UnregisterObserver(IUpdateObserver observer)
    {
        if (_pendingObservers.Contains(observer))
        {
            _pendingObservers.Remove(observer);
        }
        else if(_observers.Contains(observer))
        {
            _observersToRemove.Add(observer);
            Debug.Log($"UnRegistered the Update Method {observer}");
        }  
    }

    private void OnDestroy()
    {
        _observers.Clear();
    }
}
