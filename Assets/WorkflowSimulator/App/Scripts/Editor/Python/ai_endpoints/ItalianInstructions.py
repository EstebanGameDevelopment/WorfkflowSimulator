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
    name: str = Field(description="Nome del capitolo")
    description: str = Field(description="Riassunto degli eventi e dei personaggi del capitolo con non meno di 200 parole")


class DocumentSummary(BaseModel):
    name: str = Field(description="Nome del documento")
    type: str = Field(description="Tipo di documento (testo, immagine)")
    description: str = Field(description="Riassunto del documento in meno di 200 parole")


class MeetingReply(BaseModel):
    participant: str = Field(description="Nome del partecipante alla riunione che fornisce la risposta")
    reply: str = Field(description="Testo della risposta del partecipante alla riunione")
    end: int = Field(description="Valore (0,1): 0 indica che la riunione deve continuare, 1 indica che la riunione può terminare poiché gli argomenti principali sono stati discussi")


class MeetingDocument(BaseModel):
    name: str = Field(description="Nome del documento")
    persons: str = Field(description="Nomi delle persone che creeranno il documento (John, Cathy, Tom)")
    dependency: str = Field(description="Nome del documento che deve essere completato prima di creare questo")
    type: str = Field(description="Argomento del documento (requisiti, progettazione, codice, test)")
    time: int = Field(description="Tempo stimato per creare il documento in ore. Valore minimo: 1")
    data: str = Field(description="Descrizione testuale di circa 100 parole del contenuto del documento (requisiti, progettazione, codice)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Riassunto della riunione")
    documents: List[MeetingDocument] = Field(description="Elenco dei documenti più importanti da creare come conclusione della riunione")


class TaskDocument(BaseModel):
    name: str = Field(description="Nome del documento")
    persons: str = Field(description="Nomi delle persone che creeranno il documento (John, Cathy, Tom)")
    dependency: str = Field(description="Nome del documento che deve essere completato prima di creare questo")
    type: str = Field(description="Argomento del documento (requisiti, progettazione, codice, test)")
    time: int = Field(description="Tempo stimato per creare il documento in ore. Valore minimo: 1")
    data: str = Field(description="Descrizione testuale di circa 100 parole del contenuto del documento (requisiti, progettazione, codice)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Elenco dei documenti più importanti da creare per completare con successo questa attività")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Nome del documento")
    type: str = Field(description="Tipo di documento (testo, codice)")
    data: str = Field(description="Definizione dettagliata del documento basata sulle informazioni fornite")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Nome del documento")
    tasks: str = Field(description="Nomi delle altre attività (attività 1, attività 3, ecc.) in cui il documento potrebbe essere necessario per completarle")


class FeatureDescription(BaseModel):
    name: str = Field(description="Nome della funzionalità da implementare")
    description: str = Field(description="Descrizione della funzionalità da implementare in uno sprint del progetto")


class TaskDefinition(BaseModel):
    name: str = Field(description="Nome dell’attività")
    employees: str = Field(description="Nomi dei dipendenti assegnati all’attività (John, Cathy, Tom)")
    dependency: str = Field(description="Nome dell’attività, se presente, che deve essere completata prima di iniziare questa")
    type: str = Field(description="Tipo di attività (requisiti, progettazione, programmazione, test)")
    time: int = Field(description="Tempo stimato per completare l’attività in ore")
    data: str = Field(description="Descrizione testuale di almeno 150 parole sull’obiettivo dell’attività")


class TasksForSprint(BaseModel):
    name: str = Field(description="Nome della bacheca dello sprint")
    tasks: List[TaskDefinition] = Field(description="Elenco delle attività da svolgere per completare la funzionalità dello sprint in una settimana")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Nome della riunione")
    task: str = Field(description="Nome dell’attività collegata alla riunione")
    persons: str = Field(description="Nomi delle persone che parteciperanno alla riunione")
    starting: str = Field(description="Orario di inizio della riunione (AAAA/MM/GG HH:MM) (ad esempio: 2025/12/12 15:00)")
    duration: int = Field(description="Durata della riunione in minuti")
    description: str = Field(description="Descrizione testuale di almeno 150 parole sull’obiettivo della riunione")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Nome della bacheca dello sprint")
    meetings: List[MeetingDefinition] = Field(description="Elenco delle riunioni da tenere per completare la funzionalità dello sprint in una settimana")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Nome dello sprint")
    description: str = Field(description="Descrizione dello sprint del progetto")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Nome del progetto")
    description: str = Field(description="Descrizione del progetto")


class MeetingForTask(BaseModel):
    name: str = Field(description="Nome della riunione")
    description: str = Field(description="Descrizione degli obiettivi della riunione")
    task: str = Field(description="Nome dell’attività per la quale si svolge la riunione")
    time: int = Field(description="Durata stimata della riunione in minuti")
    persons: str = Field(description="Persone che devono partecipare alla riunione (John, Cathy, Tom)")


class GroupCompany(BaseModel):
    name: str = Field(description="Nome del gruppo")
    description: str = Field(description="Descrizione dei compiti che il gruppo di persone deve svolgere all’interno dell’azienda")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Nome del dipendente")
    sex: str = Field(description="Sesso del dipendente (uomo, donna)")
    group: str = Field(description="Nome del gruppo a cui appartiene il dipendente")
    category: str = Field(description="Categoria del dipendente (Capo, Senior, Normale)")
    skills: str = Field(description="Descrizione delle competenze del dipendente")
    personality: str = Field(description="Descrizione della personalità del dipendente insieme a 2 hobby")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Nome di un possibile progetto che può essere sviluppato da questa azienda")
    projectdescription: str = Field(description="Descrizione del possibile progetto che può essere sviluppato da questa azienda")
    groups: List[GroupCompany] = Field(description="Elenco dei gruppi dell’azienda")
    employees: List[EmployeeCompany] = Field(description="Elenco dei dipendenti dell’azienda")
   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_en.it'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/it'  # Set this to your desired directory
        self.templateQuestion = """Nella lingua italiana l’AI deve seguire le istruzioni e le richieste impartite dall’utente umano.

                        Conversazione corrente:
                        {history}
                        Utente: {input}
                        Assistente IA:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="In italiano, rispondere alla richiesta dell'utente.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """L'IA deve tradurre il testo contenuto nel tag XML <textsource> in italiano.

                    Conversazione corrente:
                    {history}
                    <textsource> {input} </textsource>
                    Assistente IA:"""   

