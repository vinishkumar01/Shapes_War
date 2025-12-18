using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateUpdateManager : MonoBehaviour
{
    private static List<ILateUpdateObserver> _observers = new List<ILateUpdateObserver>();
    private static List<ILateUpdateObserver> _pendingObservers = new List<ILateUpdateObserver>();
    private static List<ILateUpdateObserver> _observersToRemove = new List<ILateUpdateObserver>();    

    private static int _currentIndex;

    public static LateUpdateManager _instance;

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
            //Debug.Log($"Removed the Update Method {_observersToRemove[i]} at index {actualIndex} (loop i = {i})");
        }
        _observersToRemove.Clear();

        for(_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex-- )
        {
            _observers[_currentIndex]?.ObservedLateUpdate();
        }

        _observers.AddRange(_pendingObservers);
        _pendingObservers.Clear();
    }


    public static void RegisterObserver(ILateUpdateObserver observer)
    {
        if(!_observers.Contains(observer) && !_pendingObservers.Contains(observer))
        {
            _pendingObservers.Add(observer);
            //Debug.Log($"Added the Update Method {observer}");
        }    
    }

    public static void UnregisterObserver(ILateUpdateObserver observer)
    {
        if (_pendingObservers.Contains(observer))
        {
            _pendingObservers.Remove(observer);
        }
        else if(_observers.Contains(observer))
        {
            _observersToRemove.Add(observer);
            //Debug.Log($"UnRegistered the Update Method {observer}");
        }  
    }

    private void OnDestroy()
    {
        _observers.Clear();
    }
}
