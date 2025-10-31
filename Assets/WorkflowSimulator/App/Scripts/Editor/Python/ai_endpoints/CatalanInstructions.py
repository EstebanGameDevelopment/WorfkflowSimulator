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
    name: str = Field(description="Nom del capítol")
    description: str = Field(description="Resum dels esdeveniments i personatges del capítol amb no menys de 200 paraules")


class DocumentSummary(BaseModel):
    name: str = Field(description="Nom del document")
    type: str = Field(description="Tipus de document (text, imatge)")
    description: str = Field(description="Resum del document amb menys de 200 paraules")


class MeetingReply(BaseModel):
    participant: str = Field(description="Nom del participant de la reunió que dona la resposta")
    reply: str = Field(description="Text de la resposta del participant de la reunió")
    end: int = Field(description="Valor (0,1): 0 la reunió ha de continuar, 1 la reunió pot finalitzar ja que s’han tractat els temes principals")


class MeetingDocument(BaseModel):
    name: str = Field(description="Nom del document")
    persons: str = Field(description="Noms de les persones que crearan el document (John, Cathy, Tom)")
    dependency: str = Field(description="Nom del document que s’ha de completar abans de crear aquest")
    type: str = Field(description="Tema del document (requeriments, disseny, codi, proves)")
    time: int = Field(description="Estimació del temps per crear el document en hores. Valor mínim: 1")
    data: str = Field(description="Descripció textual d’unes 100 paraules sobre el contingut del document (requeriments, disseny, codi)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Resum de la reunió")
    documents: List[MeetingDocument] = Field(description="Llista dels documents més importants que s’han de crear com a conclusió de la reunió")


class TaskDocument(BaseModel):
    name: str = Field(description="Nom del document")
    persons: str = Field(description="Noms de les persones que crearan el document (John, Cathy, Tom)")
    dependency: str = Field(description="Nom del document que s’ha de completar abans de crear aquest")
    type: str = Field(description="Tema del document (requeriments, disseny, codi, proves)")
    time: int = Field(description="Estimació del temps per crear el document en hores. Valor mínim: 1")
    data: str = Field(description="Descripció textual d’unes 100 paraules sobre el contingut del document (requeriments, disseny, codi)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Llista dels documents més importants que s’han de crear per completar amb èxit aquesta tasca")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Nom del document")
    type: str = Field(description="Tipus de document (text, codi)")
    data: str = Field(description="Definició detallada del document basada en la informació proporcionada")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Nom del document")
    tasks: str = Field(description="Noms d’altres tasques (tasca 1, tasca 3, etc.) en les quals pot ser necessari considerar aquest document per completar-les")


class FeatureDescription(BaseModel):
    name: str = Field(description="Nom de la funcionalitat a implementar")
    description: str = Field(description="Descripció de la funcionalitat a implementar en un sprint del projecte")


class TaskDefinition(BaseModel):
    name: str = Field(description="Nom de la tasca")
    employees: str = Field(description="Noms dels empleats assignats a la tasca (John, Cathy, Tom)")
    dependency: str = Field(description="Nom de la tasca, si n’hi ha alguna, que s’ha de completar abans de començar aquesta")
    type: str = Field(description="Tipus de tasca (requeriments, disseny, programació, proves)")
    time: int = Field(description="Estimació del temps per completar la tasca en hores")
    data: str = Field(description="Descripció textual, amb almenys 150 paraules, de l’objectiu de la tasca")


class TasksForSprint(BaseModel):
    name: str = Field(description="Nom del tauler de l’sprint")
    tasks: List[TaskDefinition] = Field(description="Llista de les tasques a realitzar per completar la funcionalitat de l’sprint en una setmana")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Nom de la reunió")
    task: str = Field(description="Nom de la tasca vinculada a la reunió")
    persons: str = Field(description="Noms de les persones que assistiran a la reunió")
    starting: str = Field(description="Hora d’inici de la reunió (AAAA/MM/DD HH:MM) (per exemple: 2025/12/12 15:00)")
    duration: int = Field(description="Durada de la reunió en minuts")
    description: str = Field(description="Descripció textual, amb almenys 150 paraules, de l’objectiu de la reunió")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Nom del tauler de l’sprint")
    meetings: List[MeetingDefinition] = Field(description="Llista de les reunions a realitzar per completar la funcionalitat de l’sprint en una setmana")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Nom de l’sprint")
    description: str = Field(description="Descripció de l’sprint del projecte")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Nom del projecte")
    description: str = Field(description="Descripció del projecte")


class MeetingForTask(BaseModel):
    name: str = Field(description="Nom de la reunió")
    description: str = Field(description="Descripció dels objectius de la reunió")
    task: str = Field(description="Nom de la tasca per a la qual es fa la reunió")
    time: int = Field(description="Durada estimada de la reunió en minuts")
    persons: str = Field(description="Persones que han d’assistir a la reunió (John, Cathy, Tom)")


class GroupCompany(BaseModel):
    name: str = Field(description="Nom del grup")
    description: str = Field(description="Descripció de les funcions que el grup de persones ha de dur a terme dins de l’empresa")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Nom de l’empleat")
    sex: str = Field(description="Sexe de l’empleat (home, dona)")
    group: str = Field(description="Nom del grup al qual pertany l’empleat")
    category: str = Field(description="Categoria de l’empleat (Líder, Sènior, Normal)")
    skills: str = Field(description="Descripció de les habilitats de l’empleat")
    personality: str = Field(description="Descripció de la personalitat de l’empleat juntament amb 2 aficions")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Nom d’un possible projecte que pot desenvolupar aquesta empresa")
    projectdescription: str = Field(description="Descripció del possible projecte que pot desenvolupar aquesta empresa")
    groups: List[GroupCompany] = Field(description="Llista dels grups de l’empresa")
    employees: List[EmployeeCompany] = Field(description="Llista dels empleats de l’empresa")
   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_ca.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/ca'  # Set this to your desired directory
        self.templateQuestion = """En idioma Català, la IA ha de seguir les instruccions i preguntes que rep de l'humà.

                            Conversa actual:
                            {history}
                            Humà: {input}
                            Assistent IA:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="En idioma Català, respon a la següent petició de l'usuari.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRADUIR TEXT ++ 
        self.templateTranslation = """La IA ha de traduir el text contingut dins de l'etiqueta XML <textsource> a l'idioma català.

                        Conversa actual:
                        {history}
                        <textsource> {input} </textsource>
                        Assistent IA:"""

