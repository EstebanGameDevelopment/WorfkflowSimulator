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
    name: str = Field(description="Name des Kapitels")
    description: str = Field(description="Zusammenfassung der Ereignisse und Charaktere des Kapitels mit nicht weniger als 200 Wörtern")


class DocumentSummary(BaseModel):
    name: str = Field(description="Name des Dokuments")
    type: str = Field(description="Typ des Dokuments (Text, Bild)")
    description: str = Field(description="Zusammenfassung des Dokuments in weniger als 200 Wörtern")


class MeetingReply(BaseModel):
    participant: str = Field(description="Name des Teilnehmers des Meetings, der die Antwort gibt")
    reply: str = Field(description="Antworttext des Teilnehmers des Meetings")
    end: int = Field(description="Wert (0,1): 0 bedeutet, dass das Meeting fortgesetzt werden soll, 1 bedeutet, dass das Meeting beendet werden kann, da die Hauptthemen besprochen wurden")


class MeetingDocument(BaseModel):
    name: str = Field(description="Name des Dokuments")
    persons: str = Field(description="Namen der Personen, die das Dokument erstellen werden (John, Cathy, Tom)")
    dependency: str = Field(description="Name des Dokuments, das abgeschlossen sein muss, bevor dieses erstellt werden kann")
    type: str = Field(description="Thema des Dokuments (Anforderungen, Design, Code, Tests)")
    time: int = Field(description="Geschätzte Zeit zur Erstellung des Dokuments in Stunden. Mindestwert: 1")
    data: str = Field(description="Textbeschreibung mit 100 Wörtern über den Inhalt des Dokuments (Anforderungen, Design, Code)")


class MeetingSummary(BaseModel):
    summary: str = Field(description="Zusammenfassung des Meetings")
    documents: List[MeetingDocument] = Field(description="Liste der wichtigsten Dokumente, die als Ergebnis des Meetings erstellt werden müssen")


class TaskDocument(BaseModel):
    name: str = Field(description="Name des Dokuments")
    persons: str = Field(description="Namen der Personen, die das Dokument erstellen werden (John, Cathy, Tom)")
    dependency: str = Field(description="Name des Dokuments, das abgeschlossen sein muss, bevor dieses erstellt werden kann")
    type: str = Field(description="Thema des Dokuments (Anforderungen, Design, Code, Tests)")
    time: int = Field(description="Geschätzte Zeit zur Erstellung des Dokuments in Stunden. Mindestwert: 1")
    data: str = Field(description="Textbeschreibung mit 100 Wörtern über den Inhalt des Dokuments (Anforderungen, Design, Code)")


class TasksDocumentsTODO(BaseModel):
    documents: List[MeetingDocument] = Field(description="Liste der wichtigsten Dokumente, die erstellt werden müssen, um diese Aufgabe erfolgreich abzuschließen")


class DocumentGeneration(BaseModel):
    name: str = Field(description="Name des Dokuments")
    type: str = Field(description="Typ des Dokuments (Text, Code)")
    data: str = Field(description="Detaillierte Definition des Dokuments basierend auf den bereitgestellten Informationen")


class DocumentGlobal(BaseModel):
    name: str = Field(description="Name des Dokuments")
    tasks: str = Field(description="Namen anderer Aufgaben (Aufgabe 1, Aufgabe 3 usw.), bei denen das Dokument möglicherweise berücksichtigt werden muss, um sie abzuschließen")


class FeatureDescription(BaseModel):
    name: str = Field(description="Name der zu implementierenden Funktion")
    description: str = Field(description="Beschreibung der Funktion, die in einem Sprint für ein Projekt implementiert werden soll")


class TaskDefinition(BaseModel):
    name: str = Field(description="Name der Aufgabe")
    employees: str = Field(description="Namen der Mitarbeiter, die der Aufgabe zugewiesen werden (John, Cathy, Tom)")
    dependency: str = Field(description="Name der Aufgabe, falls vorhanden, die abgeschlossen sein muss, bevor diese beginnen kann")
    type: str = Field(description="Typ der Aufgabe (Anforderungen, Design, Programmierung, Tests)")
    time: int = Field(description="Geschätzte Zeit zur Fertigstellung der Aufgabe in Stunden")
    data: str = Field(description="Textbeschreibung mit mindestens 150 Wörtern über das Ziel der Aufgabe")


class TasksForSprint(BaseModel):
    name: str = Field(description="Name des Sprint-Boards")
    tasks: List[TaskDefinition] = Field(description="Liste der Aufgaben, die erledigt werden müssen, um die Sprint-Funktion innerhalb einer Woche abzuschließen")


class MeetingDefinition(BaseModel):
    name: str = Field(description="Name des Meetings")
    task: str = Field(description="Name der Aufgabe, die mit dem Meeting verknüpft ist")
    persons: str = Field(description="Namen der Personen, die am Meeting teilnehmen werden")
    starting: str = Field(description="Startzeit des Meetings (JJJJ/MM/TT HH:MM) (zum Beispiel: 2025/12/12 15:00)")
    duration: int = Field(description="Dauer des Meetings in Minuten")
    description: str = Field(description="Textbeschreibung mit mindestens 150 Wörtern über das Ziel des Meetings")


class MeetingsPlanning(BaseModel):
    name: str = Field(description="Name des Sprint-Boards")
    meetings: List[MeetingDefinition] = Field(description="Liste der Meetings, die durchgeführt werden müssen, um die Sprint-Funktion innerhalb einer Woche abzuschließen")


class BoardSprintDefinition(BaseModel):
    name: str = Field(description="Name des Sprints")
    description: str = Field(description="Beschreibung des Sprints für das Projekt")


class ProjectDefinition(BaseModel):
    name: str = Field(description="Name des Projekts")
    description: str = Field(description="Beschreibung des Projekts")


class MeetingForTask(BaseModel):
    name: str = Field(description="Name des Meetings")
    description: str = Field(description="Beschreibung der Ziele des Meetings")
    task: str = Field(description="Name der Aufgabe, für die das Meeting durchgeführt wird")
    time: int = Field(description="Geschätzte Dauer des Meetings in Minuten")
    persons: str = Field(description="Personen, die am Meeting teilnehmen sollen (John, Cathy, Tom)")


class GroupCompany(BaseModel):
    name: str = Field(description="Name der Gruppe")
    description: str = Field(description="Beschreibung der Aufgaben, die die Gruppe von Personen im Unternehmen ausführt")


class EmployeeCompany(BaseModel):
    name: str = Field(description="Name des Mitarbeiters")
    sex: str = Field(description="Geschlecht des Mitarbeiters (Mann, Frau)")
    group: str = Field(description="Name der Gruppe, zu der der Mitarbeiter gehört")
    category: str = Field(description="Kategorie des Mitarbeiters (Leiter, Senior, Normal)")
    skills: str = Field(description="Beschreibung der Fähigkeiten des Mitarbeiters")
    personality: str = Field(description="Beschreibung der Persönlichkeit des Mitarbeiters zusammen mit 2 Hobbys")


class TeamCompany(BaseModel):
    projectname: str = Field(description="Name eines möglichen Projekts, das von diesem Unternehmen entwickelt werden kann")
    projectdescription: str = Field(description="Beschreibung des möglichen Projekts, das von diesem Unternehmen entwickelt werden kann")
    groups: List[GroupCompany] = Field(description="Liste der Gruppen des Unternehmens")
    employees: List[EmployeeCompany] = Field(description="Liste der Mitarbeiter des Unternehmens")
   
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
        self.databaseAlchemy = 'sqlite:///workflowsimulator_de.db'
        self.voicesLanguage = '/home/esteban/Workspace/Flask/wav_voices/de'  # Set this to your desired directory
        self.templateQuestion = """In der deutsch Sprache soll die KI den Anweisungen und Anfragen des Menschen folgen.
                        Aktuelles Gespräch:
                        {history}
                        Mensch: {input}
                        KI-Assistent:"""

        # (++ CHAPTERS EXAMPLE ++)
        # Set up a parser + inject instructions into the prompt template.
        self.parserChapters = JsonOutputParser(pydantic_object=ChapterDescription)
        self.promptChapters = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserChapters.get_format_instructions()},
        )

        # DOCUMENT SUMMARY
        self.parserDocumentSummary = JsonOutputParser(pydantic_object=DocumentSummary)
        self.promptDocumentSummary = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentSummary.get_format_instructions()},
        )

        # MEETING REPLY TEXT
        self.parserMeetingReply = JsonOutputParser(pydantic_object=MeetingReply)
        self.promptMeetingReply = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingReply.get_format_instructions()},
        )

        # MEETING SUMMARY
        self.parserMeetingSummary = JsonOutputParser(pydantic_object=MeetingSummary)
        self.promptMeetingSummary = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingSummary.get_format_instructions()},
        )

        # TASK DOCUMENTS
        self.parserTasksDocumentsTODO = JsonOutputParser(pydantic_object=TasksDocumentsTODO)
        self.promptTasksDocumentsTODO = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksDocumentsTODO.get_format_instructions()},
        )

        # DOCUMENT TEXT GENERATION
        self.parserDocumentGeneration = JsonOutputParser(pydantic_object=DocumentGeneration)
        self.promptDocumentGeneration = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserDocumentGeneration.get_format_instructions()},
        )

        # MAKE LOCAL DOCUMENTS GLOBAL
        self.parserMakeDocumentsGlobal = JsonOutputParser(pydantic_object=DocumentGlobal)
        self.promptMakeDocumentsGlobal = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMakeDocumentsGlobal.get_format_instructions()},
        )
        
        # MAKE FEATURES
        self.parserFeatureDescription = JsonOutputParser(pydantic_object=FeatureDescription)
        self.promptFeatureDescription = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserFeatureDescription.get_format_instructions()},
        )

        # CREATE TASKS FOR FEATURE
        self.parserTasksForSprint = JsonOutputParser(pydantic_object=TasksForSprint)
        self.promptTasksForSprint = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTasksForSprint.get_format_instructions()},
        )
        
        # SPRINT BOARD DESCRIPTION
        self.parserBoardSprintDefinition = JsonOutputParser(pydantic_object=BoardSprintDefinition)
        self.promptBoardSprintDefinition = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserBoardSprintDefinition.get_format_instructions()},
        )        
        
        # PROJECT DESCRIPTION
        self.parserProjectDefinition = JsonOutputParser(pydantic_object=ProjectDefinition)
        self.promptProjectDefinition = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserProjectDefinition.get_format_instructions()},
        )        

        # MEETINGS FOR TASK
        self.parserMeetingForTask = JsonOutputParser(pydantic_object=MeetingForTask)
        self.promptMeetingForTask = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserMeetingForTask.get_format_instructions()},
        )        

        # TEAM FOR COMPANY
        self.parserTeamCompany = JsonOutputParser(pydantic_object=TeamCompany)
        self.promptTeamCompany = PromptTemplate(
            template="In deutsch Sprache, beantworte die Anfrage des Nutzers.\n{format_instructions}\n{query}\n",
            input_variables=["query"],
            partial_variables={"format_instructions": self.parserTeamCompany.get_format_instructions()},
        )        

        # ++++++++++++++++++++
        # ++ TRANSLATE TEXT ++ 
        self.templateTranslation = """Die KI muss den Text, der im XML-Tag <textsource> enthalten ist, ins Deutsche übersetzen.
        
                    Aktuelles Gespräch:
                    {history}
                    <textsource> {input} </textsource>
                    KI-Assistent:"""   
