using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CourseProps
{
    public string courseName;
    public List<GameObject> props;
    
    public CourseProps(string name)
    {
        courseName = name;
        props = new List<GameObject>();
    }
}

public class MascotProps : MonoBehaviour
{
    [Header("Course Props Configuration")]
    public List<CourseProps> coursePropsList = new List<CourseProps>();
    
    
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    
    
    
    void Start()
    {
        HideAllProps();
    }
    
    public CourseProps GetCourse(string courseName)
    {
        return coursePropsList.Find(course => course.courseName == courseName);
    }
    
    public void PrintAllProps()
    {
        foreach (CourseProps course in coursePropsList)
        {
            Debug.Log($"Course: {course.courseName}");
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    Debug.Log($"  - {prop.name}");
                }
                else
                {
                    Debug.Log($"  - [Empty GameObject slot]");
                }
            }
        }
    }
    
    public List<GameObject> GetPropsForCourse(string courseName)
    {
        CourseProps course = GetCourse(courseName);
        List<GameObject> props = new List<GameObject>();
        
        if (course != null)
        {
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    props.Add(prop);
                }
            }
        }
        
        return props;
    }
    
    #region Animation Events
    public void HideAllProps()
    {
        foreach (CourseProps course in coursePropsList)
        {
            if (course == null) continue;
            
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    prop.SetActive(false);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("All props have been hidden (SetActive false)");
        }
    }
    
    public void ShowAllProps()
    {
        foreach (CourseProps course in coursePropsList)
        {
            if (course == null) continue;
            
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    prop.SetActive(true);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("All props have been shown (SetActive true)");
        }
    }
    
    public void HidePropsForCourse(string courseName)
    {
        CourseProps course = GetCourse(courseName);
        if (course != null)
        {
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    prop.SetActive(false);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Hidden all props for course: {courseName}");
            }
        }
    }
    
    public void ShowPropsForCourse(string courseName)
    {
        CourseProps course = GetCourse(courseName);
        if (course != null)
        {
            foreach (GameObject prop in course.props)
            {
                if (prop != null)
                {
                    prop.SetActive(true);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Shown all props for course: {courseName}");
            }
        }
    }
    
    public void ShowQuest3()
    {        
        ShowPropsForCourse("Quest3");
        
        if (showDebugInfo)
        {
            Debug.Log("Show Quest3");
        }
    }
    

    public void ShowSolarSystem()
    {        
        ShowPropsForCourse("SolarSystem");
        
        if (showDebugInfo)
        {
            Debug.Log("Show SolarSystem");
        }
    }

    public void ShowGalaxy()
    {        
        ShowPropsForCourse("Galaxy");
        
        if (showDebugInfo)
        {
            Debug.Log("Show Galaxy");
        }
    }

    public void HideGalaxy()
    {        
        HidePropsForCourse("Galaxy");
        
        if (showDebugInfo)
        {
            Debug.Log("Hide Galaxy");
        }
    }

    #endregion
    
    
    
}
