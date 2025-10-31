namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataEnglish
    {
        // DOCUMENT SUMMARY
        public const string documentSummaryJsonString = @"
        {
            ""name"": ""Name of document"",
            ""type"": ""Type of the document(text,image)"",
            ""description"": ""Summary of the document in less than 200 words""
        }";

        // REPLY MEETING
        public const string replyMeetingJsonString = @"
        {
            ""participant"": ""Name of the participant of the meeting who does the reply"",
            ""reply"": ""Reply text of the participant of the meeting"",
            ""end"": ""Value (0,1): 0 the meeting should continue, 1 the meeting can end since the main topics have been discussed"",
        }";

        public const string documentMeetingJsonString = @"
        {
            ""name"": ""Name of document"",
            ""type"": ""Type of the document(text,image)"",
            ""data"": ""Data in form of detailed technical description like technical text, code or a text description of an image that will show that detailed data""
        }";

        // DATABASE
        public const string summaryMeetingJsonString = @"[
        {
            ""summary"": ""Detailed summary of the meeting"",
            ""documents"": [
                { 
                    ""name"": ""Name of document A"",
                    ""person"": ""John"",
                    ""dependency"": """",
                    ""type"": ""requirements"",
                    ""time"": ""1"",
                    ""data"": ""Description of the requirements that the feature should have based on the meeting's conclusions"",
                },
                { 
                    ""name"": ""Name of document B"",
                    ""person"": ""Cathy, Tom"",
                    ""dependency"": ""Name of document A"",
                    ""type"": ""design"",
                    ""time"": ""3"",
                    ""data"": ""Description of the design that the feature should be based on the meeting's conclusions"",
                },
                { 
                    ""name"": ""Name of document C"",
                    ""person"": ""Jonas,Steve,Betty"",
                    ""dependency"": ""Name of document B"",
                    ""type"": ""code"",
                    ""time"": ""5"",
                    ""data"": ""Description of the code that the feature should implement based on the meeting's conclusions"",
                },
                { 
                    ""name"": ""Name of document D"",
                    ""person"": ""Alan,Jennifer"",
                    ""dependency"": ""Name of document C"",
                    ""type"": ""testing"",
                    ""time"": ""2"",
                    ""data"": ""Description of the testing steps that should be done to verify the result based on the meeting's conclusions"",
                }
            ]
        }
]";

        public const string summaryTasksJsonString = @"[
        {
            ""documents"": [
                { 
                    ""name"": ""Name of document A"",
                    ""person"": ""John"",
                    ""dependency"": """",
                    ""type"": ""requirements"",
                    ""time"": ""1"",
                    ""data"": ""Description of the requirements that the feature should have based on the task description"",
                },
                { 
                    ""name"": ""Name of document B"",
                    ""person"": ""Cathy, Tom"",
                    ""dependency"": ""Name of document A"",
                    ""type"": ""design"",
                    ""time"": ""3"",
                    ""data"": ""Description of the design that the feature should be based on the task description"",
                },
                { 
                    ""name"": ""Name of document C"",
                    ""person"": ""Jonas,Steve,Betty"",
                    ""dependency"": ""Name of document B"",
                    ""type"": ""code"",
                    ""time"": ""5"",
                    ""data"": ""Description of the code that the feature should implement based on the task description"",
                },
                { 
                    ""name"": ""Name of document D"",
                    ""person"": ""Alan,Jennifer"",
                    ""dependency"": ""Name of document C"",
                    ""type"": ""testing"",
                    ""time"": ""2"",
                    ""data"": ""Description of the testing steps that should be done to verify the result based on the task description"",
                }
            ]
        }
]";

        public const string documentTextGeneratedJsonString = @"
        {
            ""name"": ""Name of document"",
            ""type"": ""Type of the document(text,code)"",
            ""data"": ""Detailed definition of the document based on the information provided""
        }";

        public const string globalDocumentsJsonString = @"[
        {
            ""name"": ""Document A"",
            ""tasks"": ""task 1, task 2""
        },
        {
            ""name"": ""Document B"",
            ""tasks"": ""task 3""
        }
]";

        public const string featureDescriptionJsonString = @"[
        {
            ""name"": ""Feature A"",
            ""description"": ""Detailed description of the feature A to be implemented in the next sprint of the project""
        },
        {
            ""name"": ""Feature B"",
            ""description"": ""Detailed description of the feature B to be implemented in the next sprint of the project""
        },
        {
            ""name"": ""Feature C"",
            ""description"": ""Detailed description of the feature C to be implemented in the next sprint of the project""
        }
]";

        public const string definitionTasksSprintJsonString = @"[
        {
            ""name"": ""Name of the sprint board"",
            ""tasks"": [
                { 
                    ""name"": ""Requirements Login System"",
                    ""employees"": ""John,Peter"",
                    ""dependency"": """",
                    ""type"": ""requeriments"",
                    ""time"": ""1"",
                    ""data"": ""Text description, with at least 150 words, of the goals of the task to define the requirements of the login system""
                },
                { 
                    ""name"": ""Design Login Screen"",
                    ""person"": ""Cathy,James"",
                    ""dependency"": ""Requirements Login System"",
                    ""type"": ""design"",
                    ""time"": ""3"",
                    ""data"": ""Text description, with at least 150 words, of the goal of the task to design the elements for the login screen""
                },
                { 
                    ""name"": ""Programming Login System"",
                    ""person"": ""Jonas,Steve,Betty"",
                    ""dependency"": ""Requirements Login System"",
                    ""type"": ""code"",
                    ""time"": ""5"",
                    ""data"": ""Text description, with at least 150 words, of the goal of the task to program the login system""
                },
                { 
                    ""name"": ""Testing Login System"",
                    ""person"": ""Harry"",
                    ""dependency"": ""Programming Login System"",
                    ""type"": ""testing"",
                    ""time"": ""2"",
                    ""data"": ""Text description, with at least 150 words, of the goal of the task to test the login system""
                }
            ]
        }
]";

        public const string documentTextSprintBoarDefinition = @"
        {
            ""name"": ""Name of the sprint"",
            ""description"": ""Description of the sprint for the project""
        }";

        public const string documentTextProjectDefinition = @"
        {
            ""name"": ""Name of the project"",
            ""description"": ""Description of the project""
        }";

        public const string meetingForTaskJsonString = @"[
        {
            ""name"": ""Requirements definition for Task A"",
            ""description"": ""Production, Design and Programming leads are meeting in order to how to define the requirements of the project"",
            ""task"": ""Task A"",
            ""time"": ""60"",
            ""persons"": ""Cathy,James,Tom,Steve""
        },
        {
            ""name"": ""Design implementation for Task A"",
            ""description"": ""The Design team is meeting with the leads of Production and Programming team to establish their framework to create a valid design"",
            ""task"": ""Task A"",
            ""time"": ""90"",
            ""persons"": ""Jonas,Christopher,Robin,Sophia""
        },
        {
            ""name"": ""Code implementation for Task A"",
            ""description"": ""The Programming team meets in order to organize how to implement the feature with the design provided"",
            ""task"": ""Task A"",
            ""time"": ""60"",
            ""persons"": ""Bill,Peter,David,Jil""
        }
]";

        /*
        public const string teamCompanyJsonString = @"
        {
            ""groups"": [
                { 
                    ""name"": ""Designers"",
                    ""description"": ""The design team will handle the visual design based on the client's requirements and technical restrictions""
                },
                { 
                    ""name"": ""Programmers"",
                    ""description"": ""The programming team will handle the implementation based on the designs provided""
                },
                { 
                    ""name"": ""Quality Assurance"",
                    ""description"": ""The quality assurance team will verify if the implemented features to check if everything has been properly implemented according to the specifications""
                },
                { 
                    ""name"": ""Producers"",
                    ""description"": ""The production team will handle the communication, organization and planning in order to synchronize the workflow of all the teams to maximize productivity""
                },
                { 
                    ""name"": ""Clients"",
                    ""description"": ""The groups of clients represents the clients of the company""
                }
            ],
            ""employees"": [
                { 
                    ""name"": ""Thomas"",
                    ""group"": ""Designers"",
                    ""category"": ""Lead"",
                    ""description"": ""An experience lead designer with plenty of experience in UI/UX design for app""
                },
                { 
                    ""name"": ""Christine"",
                    ""group"": ""Designers"",
                    ""category"": ""Senior"",
                    ""description"": ""A senior designer specialized in illustration and graphic design""
                },
                { 
                    ""name"": ""Robert"",
                    ""group"": ""Designers"",
                    ""category"": ""Normal"",
                    ""description"": ""A designer with a few years of experience in mobile app design""
                },
                { 
                    ""name"": ""William"",
                    ""group"": ""Programmers"",
                    ""category"": ""Lead"",
                    ""description"": ""An experience lead programmer with plenty of experience in multiplatform development""
                },
                { 
                    ""name"": ""Sarah"",
                    ""group"": ""Programmers"",
                    ""category"": ""Senior"",
                    ""description"": ""A senior programmer specialized in mobile app development""
                },
                { 
                    ""name"": ""Anthony"",
                    ""group"": ""Programmers"",
                    ""category"": ""Normal"",
                    ""description"": ""A programmer with a few years of experience backend programming""
                },
                { 
                    ""name"": ""Steven"",
                    ""group"": ""Quality Assurance"",
                    ""category"": ""Lead"",
                    ""description"": ""An experience lead programmer with plenty of experience in multiplatform development""
                },
                { 
                    ""name"": ""Betty"",
                    ""group"": ""Quality Assurance"",
                    ""category"": ""Lead"",
                    ""description"": ""A lead tester specialized in multiplatform testing with plenty of experience""
                },
                { 
                    ""name"": ""Joshua"",
                    ""group"": ""Quality Assurance"",
                    ""category"": ""Senior"",
                    ""description"": ""A senior tester specialized in mobile testing""
                },
                { 
                    ""name"": ""Donna"",
                    ""group"": ""Quality Assurance"",
                    ""category"": ""Normal"",
                    ""description"": ""A tester with some experience in browser application testing""
                },
                { 
                    ""name"": ""Kenneth"",
                    ""group"": ""Producers"",
                    ""category"": ""Lead"",
                    ""description"": ""A lead producer specialized in multimedia and 3D interative project production""
                },
                { 
                    ""name"": ""Amanda"",
                    ""group"": ""Producers"",
                    ""category"": ""Senior"",
                    ""description"": ""A senior producer specialized in multiplayer production""
                },
                { 
                    ""name"": ""Jason"",
                    ""group"": ""Producers"",
                    ""category"": ""Normal"",
                    ""description"": ""A producer with some experience developing mobile apps""
                },
                { 
                    ""name"": ""Sharon"",
                    ""group"": ""Clients"",
                    ""category"": ""Normal"",
                    ""description"": ""A client who wants the team to develop a mobile application""
                },
                { 
                    ""name"": ""Cynthia"",
                    ""group"": ""Clients"",
                    ""category"": ""Normal"",
                    ""description"": ""A client who wants the team to develop a VR multiplayer application""
                }
            ]
        }";
        */

        public const string teamCompanyJsonString = @"
        {
            ""projectname"": ""Name of a possible project that can be developed by this company"",
            ""projectdescription"": ""Description of the possible project that can be developed by this company"",
            ""groups"": [
                { 
                    ""name"": ""Group A"",
                    ""description"": ""The group A will handle a part of the project""
                },
                { 
                    ""name"": ""Group B"",
                    ""description"": ""The group B will handle another part of the project""
                },
                { 
                    ""name"": ""Group C"",
                    ""description"": ""The group C will handle yet another part of the project""
                },
                { 
                    ""name"": ""Clients"",
                    ""description"": ""The groups of clients represents the clients of the company""
                }
            ],
            ""employees"": [
                { 
                    ""name"": ""Name of employee X"",
                    ""sex"": ""man"",
                    ""group"": ""Employee X's group"",
                    ""category"": ""Lead"",
                    ""skills"": ""An experience lead employee that will be responsible of his group"",
                    ""personality"": ""He is serious person and responsible. He tries to lead with respect. In his free time he enjoys biking.""
                },
                { 
                    ""name"": ""Name of the employee Y"",
                    ""sex"": ""woman"",
                    ""group"": ""Employee Y's group"",
                    ""category"": ""Senior"",
                    ""skills"": ""A senior employee that will contribute to his group with experience"",
                    ""personality"": ""She is a relaxed person who likes to work at her own pace. She isn't much of a team player. In her free time she enjoys cinema and theater.""
                },
                { 
                    ""name"": ""Name of the employee Z"",
                    ""sex"": ""man"",
                    ""group"": ""Employee Y's group"",
                    ""category"": ""Normal"",
                    ""skills"": ""A normal employee with just a few years of experience"",
                    ""personality"": ""He is a anxious person who is worried about the quality of the job he does. He tries to follow the orders of the lead as best as he can. In his free time he enjoys videogames.""
                },
                { 
                    ""name"": ""Name of the client"",
                    ""sex"": ""woman"",
                    ""group"": ""Clients"",
                    ""category"": ""Normal"",
                    ""skills"": ""A client who wants the team to develop a his requested project"",
                    ""personality"": ""She is a demanding person who is trying to prove to the world how good she is. In her free time she likes Opera and expensive restaurants.""
                }
            ]
        }";
    }
}