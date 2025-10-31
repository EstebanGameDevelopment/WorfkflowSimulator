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
    name: str = Field(description="Название главы")
    description: str = Field(description="Краткое содержание событий и персонажей главы не менее чем на 200 слов")


class DocumentSummary(BaseModel):
    name: str = Field(description="Название документа")
    type: str = Field(description="Тип документа (текст, изображение)")
    description: str = Field(description="Краткое описание документа менее чем на 200 слов")


class MeetingReply(BaseModel):
    participant: str = Field(description="Имя участника встречи, который дает ответ")
    reply: str = Field(description="Текст ответа участника встречи")
    end: int = Field(description="Значение (0,1): 0 — встреча должна продолжаться, 1 — встречу можно завершить, так как основные темы обсуждены")


class MeetingDocument(BaseModel):
    name: str = Field(description="Название документа")
    persons: str = Field(description="Имена людей, которые будут создавать документ (Джон, Кэти, Том)")
    dependency: str = Field(description="Название документа, который должен быть завершён до создания этого")
    type: str = Field(description="Тема документа (требования, проектирование, код, тестирование)")
    time: int = Field(description="Оценка времени на создание документа в часах. Минимальное значение: 1")
    data: str = Field(description="Текстовое описание из 100 слов о содержании документа (требования, проектирование, код)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Резюме встречи")
    documents: List[MeetingDocument] = Field(description="Список наиболее важных документов, которые необходимо создать по итогам встречи")


class TaskDocument(BaseModel):
    name: str = Field(description="Название документа")
    persons: str = Field(description="Имена людей, которые будут создавать документ (Джон, Кэти, Том)")
    dependency: str = Field(description="Название документа, который должен быть завершён до создания этого")
    type: str = Field(description="Тема документа (требования, проектирование, код, тестирование)")
    time: int = Field(description="Оценка времени на создание документа в часах. Минимальное значение: 1")
    data: str = Field(description="Текстовое описание из 100 слов о содержании документа (требования, проектирование, код)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Список наиболее важных документов, которые необходимо создать для успешного выполнения этой задачи")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Название документа")
    type: str = Field(description="Тип документа (текст, код)")
    data: str = Field(description="Подробное определение документа на основе предоставленной информации")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Название документа")
    tasks: str = Field(description="Названия других задач (задача 1, задача 3 и т.д.), для которых документ может быть полезен при их выполнении")


class FeatureDescription(BaseModel):
    name: str = Field(description="Название функции для реализации")
    description: str = Field(description="Описание функции, которую нужно реализовать в спринте проекта")


class TaskDefinition(BaseModel):
    name: str = Field(description="Название задачи")
    employees: str = Field(description="Имена сотрудников, назначенных на задачу (Джон, Кэти, Том)")
    dependency: str = Field(description="Название задачи, если таковая имеется, которую необходимо завершить перед началом этой")
    type: str = Field(description="Тип задачи (требования, проектирование, программирование, тестирование)")
    time: int = Field(description="Оценка времени для выполнения задачи в часах")
    data: str = Field(description="Текстовое описание цели задачи не менее чем на 150 слов")


class TasksForSprint(BaseModel):
    name: str = Field(description="Название доски спринта")
    tasks: List[TaskDefinition] = Field(description="Список задач, которые необходимо выполнить, чтобы завершить функционал спринта за 1 неделю")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Название встречи")
    task: str = Field(description="Название задачи, связанной со встречей")
    persons: str = Field(description="Имена людей, которые будут присутствовать на встрече")
    starting: str = Field(description="Время начала встречи (ГГГГ/ММ/ДД ЧЧ:ММ) (например: 2025/12/12 15:00)")
    duration: int = Field(description="Продолжительность встречи в минутах")
    description: str = Field(description="Текстовое описание цели встречи не менее чем на 150 слов")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Название доски спринта")
    meetings: List[MeetingDefinition] = Field(description="Список встреч, которые нужно провести, чтобы завершить функционал спринта за 1 неделю")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Название спринта")
    description: str = Field(description="Описание спринта для проекта")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Название проекта")
    description: str = Field(description="Описание проекта")


class MeetingForTask(BaseModel):
    name: str = Field(description="Название встречи")
    description: str = Field(description="Описание целей встречи")
    task: str = Field(description="Название задачи, для которой проводится встреча")
    time: int = Field(description="Предполагаемая продолжительность встречи в минутах")
    persons: str = Field(description="Люди, которые должны присутствовать на встрече (Джон, Кэти, Том)")


class GroupCompany(BaseModel):
    name: str = Field(description="Название группы")
    description: str = Field(description="Описание обязанностей группы сотрудников, работающих в компании")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Имя сотрудника")
    sex: str = Field(description="Пол сотрудника (мужчина, женщина)")
    group: str = Field(description="Название группы, к которой принадлежит сотрудник")
    category: str = Field(description="Категория сотрудника (Руководитель, Старший, Обычный)")
    skills: str = Field(description="Описание навыков сотрудника")
    personality: str = Field(description="Описание личности сотрудника и упоминание 2 хобби")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Название возможного проекта, который может быть разработан компанией")
    projectdescription: str = Field(description="Описание возможного проекта, который может быть разработан компанией")
    groups: List[GroupCompany] = Field(description="Список групп компании")
    employees: List[EmployeeCompany] = Field(description="Список сотрудников компании")
   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_ru.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/ru'  # Set this to your desired directory
        self.templateQuestion = """По-русски искусственный интеллект должен следовать инструкциям и запросам человека.

                        Текущий разговор:
                        {history}
                        Человек: {input}
                        ИИ-ассистент:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="Ответьте на запрос пользователя на русском языке.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """ИИ должен перевести текст, содержащийся внутри тега XML <textsource>, на русский язык.

                    Текущий разговор:
                    {history}
                    <textsource> {input} </textsource>
                    ИИ-ассистент:"""   
