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

class ChapterDescription(BaseModel):
    name: str = Field(description="Nom du chapitre")
    description: str = Field(description="Résumé des événements et des personnages du chapitre en au moins 200 mots")


class DocumentSummary(BaseModel):
    name: str = Field(description="Nom du document")
    type: str = Field(description="Type du document (texte, image)")
    description: str = Field(description="Résumé du document en moins de 200 mots")


class MeetingReply(BaseModel):
    participant: str = Field(description="Nom du participant à la réunion qui donne la réponse")
    reply: str = Field(description="Texte de la réponse du participant à la réunion")
    end: int = Field(description="Valeur (0,1) : 0 la réunion doit continuer, 1 la réunion peut se terminer car les sujets principaux ont été abordés")


class MeetingDocument(BaseModel):
    name: str = Field(description="Nom du document")
    persons: str = Field(description="Noms des personnes qui vont créer le document (John, Cathy, Tom)")
    dependency: str = Field(description="Nom du document qui doit être terminé avant de créer celui-ci")
    type: str = Field(description="Sujet du document (exigences, conception, code, tests)")
    time: int = Field(description="Estimation du temps pour créer le document en heures. Valeur minimale : 1")
    data: str = Field(description="Description textuelle d’environ 100 mots du contenu du document (exigences, conception, code)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Résumé de la réunion")
    documents: List[MeetingDocument] = Field(description="Liste des documents les plus importants à créer à la suite de la réunion")


class TaskDocument(BaseModel):
    name: str = Field(description="Nom du document")
    persons: str = Field(description="Noms des personnes qui vont créer le document (John, Cathy, Tom)")
    dependency: str = Field(description="Nom du document qui doit être terminé avant de créer celui-ci")
    type: str = Field(description="Sujet du document (exigences, conception, code, tests)")
    time: int = Field(description="Estimation du temps pour créer le document en heures. Valeur minimale : 1")
    data: str = Field(description="Description textuelle d’environ 100 mots du contenu du document (exigences, conception, code)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Liste des documents les plus importants à créer pour mener à bien cette tâche")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Nom du document")
    type: str = Field(description="Type du document (texte, code)")
    data: str = Field(description="Définition détaillée du document basée sur les informations fournies")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Nom du document")
    tasks: str = Field(description="Noms des autres tâches (tâche 1, tâche 3, etc.) pour lesquelles le document pourrait être nécessaire afin de les compléter")


class FeatureDescription(BaseModel):
    name: str = Field(description="Nom de la fonctionnalité à implémenter")
    description: str = Field(description="Description de la fonctionnalité à implémenter dans un sprint de projet")


class TaskDefinition(BaseModel):
    name: str = Field(description="Nom de la tâche")
    employees: str = Field(description="Noms des employés assignés à la tâche (John, Cathy, Tom)")
    dependency: str = Field(description="Nom de la tâche, le cas échéant, qui doit être terminée avant de commencer celle-ci")
    type: str = Field(description="Type de tâche (exigences, conception, programmation, tests)")
    time: int = Field(description="Estimation du temps pour terminer la tâche en heures")
    data: str = Field(description="Description textuelle d’au moins 150 mots sur l’objectif de la tâche")


class TasksForSprint(BaseModel):
    name: str = Field(description="Nom du tableau de sprint")
    tasks: List[TaskDefinition] = Field(description="Liste des tâches à accomplir pour terminer la fonctionnalité du sprint en 1 semaine")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Nom de la réunion")
    task: str = Field(description="Nom de la tâche liée à la réunion")
    persons: str = Field(description="Noms des personnes qui participeront à la réunion")
    starting: str = Field(description="Heure de début de la réunion (AAAA/MM/JJ HH:MM) (par exemple : 2025/12/12 15:00)")
    duration: int = Field(description="Durée de la réunion en minutes")
    description: str = Field(description="Description textuelle d’au moins 150 mots sur l’objectif de la réunion")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Nom du tableau de sprint")
    meetings: List[MeetingDefinition] = Field(description="Liste des réunions à tenir pour achever la fonctionnalité du sprint en 1 semaine")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Nom du sprint")
    description: str = Field(description="Description du sprint du projet")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Nom du projet")
    description: str = Field(description="Description du projet")


class MeetingForTask(BaseModel):
    name: str = Field(description="Nom de la réunion")
    description: str = Field(description="Description des objectifs de la réunion")
    task: str = Field(description="Nom de la tâche pour laquelle la réunion est organisée")
    time: int = Field(description="Durée estimée de la réunion en minutes")
    persons: str = Field(description="Personnes qui doivent assister à la réunion (John, Cathy, Tom)")


class GroupCompany(BaseModel):
    name: str = Field(description="Nom du groupe")
    description: str = Field(description="Description de la mission du groupe de personnes travaillant dans l’entreprise")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Nom de l’employé")
    sex: str = Field(description="Sexe de l’employé (homme, femme)")
    group: str = Field(description="Nom du groupe auquel appartient l’employé")
    category: str = Field(description="Catégorie de l’employé (Chef d’équipe, Senior, Standard)")
    skills: str = Field(description="Description des compétences de l’employé")
    personality: str = Field(description="Description de la personnalité de l’employé accompagnée de 2 passe-temps")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Nom d’un projet possible que cette entreprise peut développer")
    projectdescription: str = Field(description="Description du projet possible que cette entreprise peut développer")
    groups: List[GroupCompany] = Field(description="Liste des groupes de l’entreprise")
    employees: List[EmployeeCompany] = Field(description="Liste des employés de l’entreprise")

   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_fr.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/fr'  # Set this to your desired directory
        self.templateQuestion = """En langue française, l’IA doit suivre les instructions et demandes fournies par l’utilisateur.

                      Conversation actuelle :
                      {history}
                      Utilisateur : {input}
                      Assistant IA :"""
                      
        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="En langue française, répondre à la question de l'utilisateur.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """L'IA doit traduire le texte contenu dans la balise XML <textsource> en français.
        
                    Conversation en cours :
                    {history}
                    <textsource> {input} </textsource>
                    Assistant IA:"""   
