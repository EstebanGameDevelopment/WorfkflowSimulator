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
    name: str = Field(description="Nombre del capítulo")
    description: str = Field(description="Resumen de los eventos y personajes del capítulo con no menos de 200 palabras")


class DocumentSummary(BaseModel):
    name: str = Field(description="Nombre del documento")
    type: str = Field(description="Tipo de documento (texto, imagen)")
    description: str = Field(description="Resumen del documento en menos de 200 palabras")


class MeetingReply(BaseModel):
    participant: str = Field(description="Nombre del participante de la reunión que realiza la respuesta")
    reply: str = Field(description="Texto de la respuesta del participante de la reunión")
    end: int = Field(description="Valor (0,1): 0 la reunión debe continuar, 1 la reunión puede finalizar ya que los temas principales han sido discutidos")


class MeetingDocument(BaseModel):
    name: str = Field(description="Nombre del documento")
    persons: str = Field(description="Nombres de las personas que van a crear el documento (Juan, Cathy, Tom)")
    dependency: str = Field(description="Nombre del documento que debe completarse antes de crear este")
    type: str = Field(description="Tema del documento (requerimientos, diseño, código, pruebas)")
    time: int = Field(description="Tiempo estimado para crear el documento en horas. Valor mínimo: 1")
    data: str = Field(description="Descripción de texto de 100 palabras sobre el contenido del documento (requerimientos, diseño, código)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Resumen de la reunión")
    documents: List[MeetingDocument] = Field(description="Lista de los documentos más importantes que deben crearse como conclusión de la reunión")


class TaskDocument(BaseModel):
    name: str = Field(description="Nombre del documento")
    persons: str = Field(description="Nombres de las personas que van a crear el documento (Juan, Cathy, Tom)")
    dependency: str = Field(description="Nombre del documento que debe completarse antes de crear este")
    type: str = Field(description="Tema del documento (requerimientos, diseño, código, pruebas)")
    time: int = Field(description="Tiempo estimado para crear el documento en horas. Valor mínimo: 1")
    data: str = Field(description="Descripción de texto de 100 palabras sobre el contenido del documento (requerimientos, diseño, código)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Lista de los documentos más importantes que deben crearse para completar con éxito esta tarea")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Nombre del documento")
    type: str = Field(description="Tipo de documento (texto, código)")
    data: str = Field(description="Definición detallada del documento basada en la información proporcionada")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Nombre del documento")
    tasks: str = Field(description="Nombres de otras tareas (tarea 1, tarea 3, etc.) en las que el documento podría ser necesario considerar para completarlas")


class FeatureDescription(BaseModel):
    name: str = Field(description="Nombre de la funcionalidad a implementar")
    description: str = Field(description="Descripción de la funcionalidad a implementar en un sprint del proyecto")


class TaskDefinition(BaseModel):
    name: str = Field(description="Nombre de la tarea")
    employees: str = Field(description="Nombres de los empleados asignados a la tarea (Juan, Cathy, Tom)")
    dependency: str = Field(description="Nombre de la tarea, si existe, que debe completarse antes de comenzar esta")
    type: str = Field(description="Tipo de tarea (requerimientos, diseño, programación, pruebas)")
    time: int = Field(description="Tiempo estimado para completar la tarea en horas")
    data: str = Field(description="Descripción en texto, con al menos 150 palabras, del objetivo de la tarea")


class TasksForSprint(BaseModel):
    name: str = Field(description="Nombre del tablero del sprint")
    tasks: List[TaskDefinition] = Field(description="Lista de tareas a realizar para completar la funcionalidad del sprint en 1 semana")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Nombre de la reunión")
    task: str = Field(description="Nombre de la tarea vinculada a la reunión")
    persons: str = Field(description="Nombres de las personas que asistirán a la reunión")
    starting: str = Field(description="Hora de inicio de la reunión (AAAA/MM/DD HH:MM) (por ejemplo: 2025/12/12 15:00)")
    duration: int = Field(description="Duración de la reunión en minutos")
    description: str = Field(description="Descripción en texto, con al menos 150 palabras, del objetivo de la reunión")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Nombre del tablero del sprint")
    meetings: List[MeetingDefinition] = Field(description="Lista de reuniones a realizar para completar la funcionalidad del sprint en 1 semana")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Nombre del sprint")
    description: str = Field(description="Descripción del sprint del proyecto")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Nombre del proyecto")
    description: str = Field(description="Descripción del proyecto")


class MeetingForTask(BaseModel):
    name: str = Field(description="Nombre de la reunión")
    description: str = Field(description="Descripción de los objetivos de la reunión")
    task: str = Field(description="Nombre de la tarea para la cual se realiza la reunión")
    time: int = Field(description="Duración estimada de la reunión en minutos")
    persons: str = Field(description="Personas que deben asistir a la reunión (Juan, Cathy, Tom)")


class GroupCompany(BaseModel):
    name: str = Field(description="Nombre del grupo")
    description: str = Field(description="Descripción de las funciones que el grupo de personas realiza dentro de la empresa")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Nombre del empleado")
    sex: str = Field(description="Sexo del empleado (hombre, mujer)")
    group: str = Field(description="Nombre del grupo al que pertenece el empleado")
    category: str = Field(description="Categoría del empleado (Líder, Senior, Normal)")
    skills: str = Field(description="Descripción de las habilidades del empleado")
    personality: str = Field(description="Descripción de la personalidad del empleado junto con 2 pasatiempos")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Nombre de un posible proyecto que puede desarrollar la empresa")
    projectdescription: str = Field(description="Descripción del posible proyecto que puede desarrollar la empresa")
    groups: List[GroupCompany] = Field(description="Lista de los grupos de la empresa")
    employees: List[EmployeeCompany] = Field(description="Lista de los empleados de la empresa")
   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_es.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/es'  # Set this to your desired directory
        self.templateQuestion = """En idioma Español, la IA debe seguir las intrucciones y preguntas que recibe del humano.

                            Conversación actual:
                            {history}
                            Humano: {input}
                            Asistente IA:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="En idioma Español, responde a la siguiente petición del usuario.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """La IA debe traducir el texto contenido dentro del tag XML <textsource> al idioma Español.

                    Conversación actual:
                    {history}
                    <textsource> {input} </textsource>
                    Asistente IA:"""   
