using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
    [Serializable]
    public class DocumentSummaryJSON : IJsonValidatable
    {
        public string name;
        public string type;
        public string description;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isTypeValid = !string.IsNullOrEmpty(type);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);

            return isNameValid && isTypeValid && isDescriptionValid;
        }
    }

    [Serializable]
    public class ReplyMeetingJSON : IJsonValidatable
    {
        public string participant;
        public string reply;
        public int end;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(participant);
            bool isTypeValid = !string.IsNullOrEmpty(reply);

            return isNameValid && isTypeValid;
        }
    }

    [Serializable]
    public class DocumentMeetingJSON : IJsonValidatable
    {
        public string name;
        public string persons;
        public string dependency;
        public string type;
        public int time;
        public string data;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isPersonValid = !string.IsNullOrEmpty(persons);
            bool isTypeValid = !string.IsNullOrEmpty(type);
            bool isTimeValid = (time > 0);
            bool isDataValid = !string.IsNullOrEmpty(data);

            return isNameValid && isPersonValid && isTypeValid && isTimeValid && isDataValid;
        }

        public override string ToString()
        {
            return name + "; " + persons + "; " + dependency + "; " + type + "; " + time + "h: " + data;
        }
    }
    
    [Serializable]
    public class MeetingSummaryJSON : IJsonValidatable
    {
        public string summary;
        public List<DocumentMeetingJSON> documents;

        public bool IsValid()
        {
            bool isSummaryValid = !string.IsNullOrEmpty(summary);

            bool areDocumentsValid = true;
            foreach (var doc in documents) areDocumentsValid = areDocumentsValid && doc.IsValid();

            return isSummaryValid && areDocumentsValid;
        }
    }


    [Serializable]
    public class TaskDocumentJSON : IJsonValidatable
    {
        public string name;
        public string persons;
        public string dependency;
        public string type;
        public int time;
        public string data;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isPersonValid = !string.IsNullOrEmpty(persons);
            bool isTypeValid = !string.IsNullOrEmpty(type);
            bool isTimeValid = (time > 0);
            bool isDataValid = !string.IsNullOrEmpty(data);

            return isNameValid && isPersonValid && isTypeValid && isTimeValid && isDataValid;
        }

        public override string ToString()
        {
            return name + "; " + persons + "; " + dependency + "; " + type + "; " + time + "h: " + data;
        }
    }

    [Serializable]
    public class TasksDocumentsJSON : IJsonValidatable
    {
        public List<TaskDocumentJSON> documents;

        public bool IsValid()
        {
            bool areDocumentsValid = true;
            foreach (var doc in documents) areDocumentsValid = areDocumentsValid && doc.IsValid();

            return areDocumentsValid;
        }
    }

    [Serializable]
    public class DocumentGeneratedJSON : IJsonValidatable
    {
        public string name;
        public string type;
        public string data;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isTypeValid = !string.IsNullOrEmpty(type);
            bool isDescriptionValid = !string.IsNullOrEmpty(data);

            return isNameValid && isTypeValid && isDescriptionValid;
        }
    }


    [Serializable]
    public class GlobalDocumentListJSON
    {
        public List<GlobalDocumentJSON> documents;
    }


    [Serializable]
    public class GlobalDocumentJSON : IJsonValidatable
    {
        public string name;
        public string tasks;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isTasksValid = !string.IsNullOrEmpty(tasks);

            return isNameValid && isTasksValid;
        }
    }

    [Serializable]
    public class FeatureDescriptionListJSON
    {
        public List<FeatureDescriptionJSON> features;
    }

    [Serializable]
    public class FeatureDescriptionJSON : IJsonValidatable
    {
        public string name;
        public string description;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);

            return isNameValid && isDescriptionValid;
        }
    }

    [Serializable]
    public class TasksSprintListJSON : IJsonValidatable
    {
        public string name;
        public List<TaskSprintJSON> tasks;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);

            bool areTasksValid = true;
            foreach (var task in tasks) areTasksValid = areTasksValid && task.IsValid();

            return isNameValid && areTasksValid;
        }
    }


    [Serializable]
    public class TaskSprintJSON : IJsonValidatable
    {
        public string name;
        public string employees;
        public string dependency;
        public string type;
        public int time;
        public string data;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isEmployeesValid = !string.IsNullOrEmpty(employees);
            bool isTypeValid = !string.IsNullOrEmpty(type);
            bool isTimeValid = (time > 0);
            bool isDataValid = !string.IsNullOrEmpty(data);

            return isNameValid && isEmployeesValid && isTypeValid && isTimeValid && isDataValid;
        }
    }

    [Serializable]
    public class SprintBoardDefinitionJSON : IJsonValidatable
    {
        public string name;
        public string description;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);

            return isNameValid && isDescriptionValid;
        }
    }

    [Serializable]
    public class ProjectDefinitionJSON : IJsonValidatable
    {
        public string name;
        public string description;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);

            return isNameValid && isDescriptionValid;
        }
    }


    [Serializable]
    public class MeetingForTaskListJSON
    {
        public List<MeetingForTaskJSON> meetings;
    }

    [Serializable]
    public class MeetingForTaskJSON : IJsonValidatable
    {
        public string name;
        public string description;
        public string task;
        public int time;
        public string persons;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isTaskValid = !string.IsNullOrEmpty(task);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);
            bool isTimeValid = (time > 0);
            bool isPersonsValid = !string.IsNullOrEmpty(persons);

            return isNameValid && isTaskValid && isDescriptionValid && isTimeValid && isPersonsValid;
        }
    }


    [Serializable]
    public class TeamCompanyListJSON : IJsonValidatable
    {
        public string projectname;
        public string projectdescription;
        public List<GroupCompanyJSON> groups;
        public List<EmployeeCompanyJSON> employees;

        public bool IsValid()
        {
            bool isProjectNameValid = !string.IsNullOrEmpty(projectname);
            bool isProjectDescriptionValid = !string.IsNullOrEmpty(projectdescription);

            bool areGroupsValid = true;
            foreach (var group in groups) areGroupsValid = areGroupsValid && group.IsValid();

            bool areEmployeesValid = true;
            foreach (var employee in employees) areEmployeesValid = areEmployeesValid && employee.IsValid();

            return isProjectNameValid && isProjectDescriptionValid && areGroupsValid && areEmployeesValid;
        }
    }

    [Serializable]
    public class GroupCompanyJSON : IJsonValidatable
    {
        public string name;
        public string description;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isDescriptionValid = !string.IsNullOrEmpty(description);

            return isNameValid && isDescriptionValid;
        }
    }

    [Serializable]
    public class EmployeeCompanyJSON : IJsonValidatable
    {
        public string name;
        public string sex;
        public string group;
        public string category;
        public string skills;
        public string personality;

        public bool IsValid()
        {
            bool isNameValid = !string.IsNullOrEmpty(name);
            bool isGenderValid = !string.IsNullOrEmpty(sex);
            bool isGroupValid = !string.IsNullOrEmpty(group);
            bool isCategoryValid = !string.IsNullOrEmpty(category);
            bool isSkillsValid = !string.IsNullOrEmpty(skills);
            bool isPersonalityValid = !string.IsNullOrEmpty(personality);

            return isNameValid && isGenderValid && isSkillsValid && isPersonalityValid && isGroupValid && isCategoryValid;
        }
    }


    public static class DocumentNormalizer
    {
        public static DocumentGeneratedJSON Normalize(string json)
        {            
            var jObject = JObject.Parse(json);

            string name = jObject["name"]?.ToString();
            string type = jObject["type"]?.ToString();
            string dataString;

            var dataToken = jObject["data"];
            if (dataToken == null)
            {
                dataString = string.Empty;
            }
            else if (dataToken.Type == JTokenType.Array)
            {
                dataString = string.Join("\n\n", dataToken.ToObject<string[]>());
            }
            else
            {
                dataString = dataToken.ToString();
            }

            return new DocumentGeneratedJSON
            {
                name = name,
                type = type,
                data = dataString
            };
        }
    }
}