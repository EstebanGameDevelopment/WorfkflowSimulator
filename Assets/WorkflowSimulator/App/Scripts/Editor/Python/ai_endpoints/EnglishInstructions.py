from pydantic import BaseModel, Field
from typing import List
from langchain.schema.messages import HumanMessage, AIMessage
from langchain.chains import ConversationChain
from langchain_core.output_parsers import JsonOutputParser
from langchain_core.prompts import PromptTemplate
from langchain.memory import ConversationBufferMemory 
from langchain_core.prompts.prompt import PromptTemplate
from langchain.memory import ConversationSummaryMemory
from langchain.prompts import (
    ChatPromptTemplate,
    HumanMessagePromptTemplate,
    MessagesPlaceholder,
    SystemMessagePromptTemplate,
)

# +++++++++++++++++++++++
# (++ CHAPTERS EXAMPLE ++)
class ChapterDescription(BaseModel):
    name: str = Field(description="Chapter's name")
    description: str = Field(description="Summary of the events and characters of the chapter with no less than 200 words")

class DocumentSummary(BaseModel):
    name: str = Field(description="Name of the document")
    type: str = Field(description="Type of the document(text,image)")
    description: str = Field(description="Summary of the document in less than 200 words")

class MeetingReply(BaseModel):
    participant: str = Field(description="Name of the participant of the meeting who does the reply")
    reply: str = Field(description="Reply text of the participant of the meeting")
    end: int = Field(description="Value (0,1): 0 the meeting should continue, 1 the meeting can end since the main topics have been discussed")

class MeetingDocument(BaseModel):
    name: str = Field(description="Name of the document")
    persons: str = Field(description="The name of the persons who are going to create the document (John, Cathy, Tom)")
    dependency: str = Field(description="The name of the document that should be complete before creating this one")
    type: str = Field(description="Topic of the document(requirements,design,code,testing)")
    time: int = Field(description="Time estimation to create the document in hours. Minimal value: 1")
    data: str = Field(description="Text description of 100 words of the contents of the document (requirements,design,code)")
    
class MeetingSummary(BaseModel):
    summary: str = Field(description="The summary of the meeting")
    documents: List[MeetingDocument] = Field(description="List of the most important documents that needs to be created as a conclusion of the meeting")

class TaskDocument(BaseModel):
    name: str = Field(description="Name of the document")
    persons: str = Field(description="The name of the persons who are going to create the document (John, Cathy, Tom)")
    dependency: str = Field(description="The name of the document that should be complete before creating this one")
    type: str = Field(description="Topic of the document(requirements,design,code,testing)")
    time: int = Field(description="Time estimation to create the document in hours. Minimal value: 1")
    data: str = Field(description="Text description of 100 words of the contents of the document (requirements,design,code)")
    
class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="List of the most important documents that needs to be created as to complete successfully this task")

class DocumentGeneration(BaseModel):
    name: str = Field(description="Name of the document")
    type: str = Field(description="Type of the document(text,code)")
    data: str = Field(description="Detailed definition of the document based on the information provided")

class DocumentGlobal(BaseModel):
    name: str = Field(description="Name of the document")
    tasks: str = Field(description="Names of the other tasks (task 1, task 3, etc...) the document could be necessary to consider in order to complete them")

class FeatureDescription(BaseModel):
    name: str = Field(description="Name of the feature to implemented")
    description: str = Field(description="Description of the feature to implement in a sprint for a project")

class TaskDefinition(BaseModel):
    name: str = Field(description="Name of the task")
    employees: str = Field(description="The name of the employees who are going to be assigned to the task (John, Cathy, Tom)")
    dependency: str = Field(description="The name of the task, if any, that should be complete before starting this one")
    type: str = Field(description="Type of the task(requeriments,design,programming,testing)")
    time: int = Field(description="Time estimation to complete the task in hours")
    data: str = Field(description="Text description, with at least 150 words, of the goal of the task")
    
class TasksForSprint(BaseModel):
    name: str = Field(description="Name of the sprint board")
    tasks: List[TaskDefinition] = Field(description="List of the tasks to do in order to complete the sprint feature in 1 week")

class MeetingDefinition(BaseModel):
    name: str = Field(description="Name of the meeting")
    task: str = Field(description="Name of the task linked to the meeting")
    persons: str = Field(description="The name of the persons who are going to be attend to the meeting")
    starting: str = Field(description="Time to start the meeting (YYYY/MM/DD HH:MM) (for example: 2025/12/12 15:00)")
    duration: int = Field(description="Duration in minutes of the meeting")
    description: str = Field(description="Text description, with at least 150 words, of the goal of the meeting")
    
class MeetingsPlanning(BaseModel):
    name: str = Field(description="Name of the sprint board")
    meetings: List[MeetingDefinition] = Field(description="List of the tasks to do in order to complete the sprint feature in 1 week")

class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Name of the sprint")
    description: str = Field(description="Description of the sprint for the project")

class ProjectDefinition(BaseModel):
    name: str = Field(description="Name of the project")
    description: str = Field(description="Description of the project")    

class MeetingForTask(BaseModel):
    name: str = Field(description="Name of the meeting")
    description: str = Field(description="Description of the goals of the meeting")
    task: str = Field(description="Name of the task that the meeting is done for")
    time: int = Field(description="Estimated duration of the meeting in minutes")
    persons: str = Field(description="Persons that should assist to the meeting (John, Cathy, Tom)")

class GroupCompany(BaseModel):
    name: str = Field(description="Name of the group")
    description: str = Field(description="Description of the duty that the group of persons that working in the company must perform")

class EmployeeCompany(BaseModel):
    name: str = Field(description="Name of the employee")
    sex: str = Field(description="Sex of the employee(man,woman)")
    group: str = Field(description="Name of the group where the employee belongs")
    category: str = Field(description="Category of the employee (Lead, Senior, Normal)")
    skills: str = Field(description="Description of the skills of the employee")
    personality: str = Field(description="Description of the personality of the employee alongside 2 hobbies")
    
class TeamCompany(BaseModel):
    projectname: str = Field(description="Name of a possible project that can be developed by this company")
    projectdescription: str = Field(description="Description of the possible project that can be developed by this company")
    groups: List[GroupCompany] = Field(description="List of the groups of the company")
    employees: List[EmployeeCompany] = Field(description="List of the employees of the company")
   
# **************************************************************
# **************************************************************
# **************************************************************
# INSTRUCTIONS AI
# **************************************************************
# **************************************************************
# **************************************************************

class InstructionsAI:
    def __init__(self):
        self.urlSpeechGeneration = "http://0.0.0.0:6000"    
        self.urlImageGeneration = "http://0.0.0.0:7860"             
        self.urlFluxImageGeneration = "http://0.0.0.0:7869"
        # self.urlFluxImageGeneration = "https://f26be2c194c343be51.gradio.live"         
        self.databaseAlchemy = 'sqlite:///workflowsimulator_en.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/en'  # Set this to your desired directory
        self.templateQuestion = """In English language, the AI should follow the instructions and requests provided by the human.

                        Current conversation:
                        {history}
                        Human: {input}
                        AI Assistant:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="In English language, answer the user query.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """The AI must translate the text contained within the XML tag <textsource> into English.

                    Current conversation:
                    {history}
                    <textsource> {input} </textsource>
                    AI Assistant:"""
