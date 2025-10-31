namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataGerman
    {
        // DOKUMENTZUSAMMENFASSUNG
        public const string documentSummaryJsonString = @"
{
    ""name"": ""Name des Dokuments"",
    ""type"": ""Typ des Dokuments (Text, Bild)"",
    ""description"": ""Zusammenfassung des Dokuments in weniger als 200 Wörtern""
}";

        // ANTWORT AUF BESPRECHUNG
        public const string replyMeetingJsonString = @"
{
    ""participant"": ""Name des Teilnehmers der Besprechung, der die Antwort gibt"",
    ""reply"": ""Antworttext des Teilnehmers der Besprechung"",
    ""end"": ""Wert (0,1): 0 = Die Besprechung soll fortgesetzt werden, 1 = Die Besprechung kann beendet werden, da die Hauptthemen besprochen wurden""
}";

        public const string documentMeetingJsonString = @"
{
    ""name"": ""Name des Dokuments"",
    ""type"": ""Typ des Dokuments (Text, Bild)"",
    ""data"": ""Daten in Form einer detaillierten technischen Beschreibung, z. B. technischer Text, Code oder eine textuelle Beschreibung eines Bildes, das diese detaillierten Daten darstellt""
}";

        // DATENBANK
        public const string summaryMeetingJsonString = @"[
{
    ""summary"": ""Detaillierte Zusammenfassung der Besprechung"",
    ""documents"": [
        { 
            ""name"": ""Name von Dokument A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""Anforderungen"",
            ""time"": ""1"",
            ""data"": ""Beschreibung der Anforderungen, die die Funktion gemäß den Besprechungsergebnissen erfüllen soll""
        },
        { 
            ""name"": ""Name von Dokument B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Name von Dokument A"",
            ""type"": ""Design"",
            ""time"": ""3"",
            ""data"": ""Beschreibung des Designs, auf dem die Funktion gemäß den Besprechungsergebnissen basieren soll""
        },
        { 
            ""name"": ""Name von Dokument C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Name von Dokument B"",
            ""type"": ""Code"",
            ""time"": ""5"",
            ""data"": ""Beschreibung des Codes, den die Funktion gemäß den Besprechungsergebnissen implementieren soll""
        },
        { 
            ""name"": ""Name von Dokument D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Name von Dokument C"",
            ""type"": ""Test"",
            ""time"": ""2"",
            ""data"": ""Beschreibung der Testschritte, die durchgeführt werden sollen, um das Ergebnis gemäß den Besprechungsergebnissen zu überprüfen""
        }
    ]
}
]";

        public const string summaryTasksJsonString = @"[
{
    ""documents"": [
        { 
            ""name"": ""Name von Dokument A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""Anforderungen"",
            ""time"": ""1"",
            ""data"": ""Beschreibung der Anforderungen, die die Funktion basierend auf der Aufgabenbeschreibung erfüllen soll""
        },
        { 
            ""name"": ""Name von Dokument B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Name von Dokument A"",
            ""type"": ""Design"",
            ""time"": ""3"",
            ""data"": ""Beschreibung des Designs, auf dem die Funktion basierend auf der Aufgabenbeschreibung beruhen soll""
        },
        { 
            ""name"": ""Name von Dokument C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Name von Dokument B"",
            ""type"": ""Code"",
            ""time"": ""5"",
            ""data"": ""Beschreibung des Codes, den die Funktion basierend auf der Aufgabenbeschreibung implementieren soll""
        },
        { 
            ""name"": ""Name von Dokument D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Name von Dokument C"",
            ""type"": ""Test"",
            ""time"": ""2"",
            ""data"": ""Beschreibung der Testschritte, die durchgeführt werden sollen, um das Ergebnis gemäß der Aufgabenbeschreibung zu überprüfen""
        }
    ]
}
]";

        public const string documentTextGeneratedJsonString = @"
{
    ""name"": ""Name des Dokuments"",
    ""type"": ""Typ des Dokuments (Text, Code)"",
    ""data"": ""Detaillierte Definition des Dokuments auf Grundlage der bereitgestellten Informationen""
}";

        public const string globalDocumentsJsonString = @"[
{
    ""name"": ""Dokument A"",
    ""tasks"": ""Aufgabe 1, Aufgabe 2""
},
{
    ""name"": ""Dokument B"",
    ""tasks"": ""Aufgabe 3""
}
]";

        public const string featureDescriptionJsonString = @"[
{
    ""name"": ""Funktion A"",
    ""description"": ""Detaillierte Beschreibung der Funktion A, die im nächsten Sprint des Projekts implementiert werden soll""
},
{
    ""name"": ""Funktion B"",
    ""description"": ""Detaillierte Beschreibung der Funktion B, die im nächsten Sprint des Projekts implementiert werden soll""
},
{
    ""name"": ""Funktion C"",
    ""description"": ""Detaillierte Beschreibung der Funktion C, die im nächsten Sprint des Projekts implementiert werden soll""
}
]";

        public const string definitionTasksSprintJsonString = @"[
{
    ""name"": ""Name des Sprint-Boards"",
    ""tasks"": [
        { 
            ""name"": ""Anforderungen Login-System"",
            ""employees"": ""John, Peter"",
            ""dependency"": """",
            ""type"": ""Anforderungen"",
            ""time"": ""1"",
            ""data"": ""Textbeschreibung mit mindestens 150 Wörtern über die Ziele der Aufgabe, um die Anforderungen des Login-Systems zu definieren""
        },
        { 
            ""name"": ""Design Login-Bildschirm"",
            ""person"": ""Cathy, James"",
            ""dependency"": ""Anforderungen Login-System"",
            ""type"": ""Design"",
            ""time"": ""3"",
            ""data"": ""Textbeschreibung mit mindestens 150 Wörtern über das Ziel der Aufgabe, die Designelemente des Login-Bildschirms zu entwerfen""
        },
        { 
            ""name"": ""Programmierung Login-System"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Anforderungen Login-System"",
            ""type"": ""Code"",
            ""time"": ""5"",
            ""data"": ""Textbeschreibung mit mindestens 150 Wörtern über das Ziel der Aufgabe, das Login-System zu programmieren""
        },
        { 
            ""name"": ""Test des Login-Systems"",
            ""person"": ""Harry"",
            ""dependency"": ""Programmierung Login-System"",
            ""type"": ""Test"",
            ""time"": ""2"",
            ""data"": ""Textbeschreibung mit mindestens 150 Wörtern über das Ziel der Aufgabe, das Login-System zu testen""
        }
    ]
}
]";

        public const string documentTextSprintBoarDefinition = @"
{
    ""name"": ""Name des Sprints"",
    ""description"": ""Beschreibung des Sprints für das Projekt""
}";

        public const string documentTextProjectDefinition = @"
{
    ""name"": ""Name des Projekts"",
    ""description"": ""Beschreibung des Projekts""
}";

        public const string meetingForTaskJsonString = @"[
{
    ""name"": ""Anforderungsdefinition für Aufgabe A"",
    ""description"": ""Leiter der Produktion, des Designs und der Programmierung treffen sich, um zu besprechen, wie die Anforderungen des Projekts definiert werden sollen"",
    ""task"": ""Aufgabe A"",
    ""time"": ""60"",
    ""persons"": ""Cathy, James, Tom, Steve""
},
{
    ""name"": ""Design-Implementierung für Aufgabe A"",
    ""description"": ""Das Designteam trifft sich mit den Leitern der Produktions- und Programmierteams, um den Rahmen für ein gültiges Design festzulegen"",
    ""task"": ""Aufgabe A"",
    ""time"": ""90"",
    ""persons"": ""Jonas, Christopher, Robin, Sophia""
},
{
    ""name"": ""Code-Implementierung für Aufgabe A"",
    ""description"": ""Das Programmierteam trifft sich, um zu organisieren, wie die Funktion mit dem bereitgestellten Design implementiert werden soll"",
    ""task"": ""Aufgabe A"",
    ""time"": ""60"",
    ""persons"": ""Bill, Peter, David, Jil""
}
]";

        public const string teamCompanyJsonString = @"
{
    ""projectname"": ""Name eines möglichen Projekts, das von diesem Unternehmen entwickelt werden kann"",
    ""projectdescription"": ""Beschreibung des möglichen Projekts, das von diesem Unternehmen entwickelt werden kann"",
    ""groups"": [
        { 
            ""name"": ""Gruppe A"",
            ""description"": ""Gruppe A wird einen Teil des Projekts übernehmen""
        },
        { 
            ""name"": ""Gruppe B"",
            ""description"": ""Gruppe B wird einen weiteren Teil des Projekts übernehmen""
        },
        { 
            ""name"": ""Gruppe C"",
            ""description"": ""Gruppe C wird einen weiteren Teil des Projekts übernehmen""
        },
        { 
            ""name"": ""Kunden"",
            ""description"": ""Die Kundengruppen repräsentieren die Kunden des Unternehmens""
        }
    ],
    ""employees"": [
        { 
            ""name"": ""Name des Mitarbeiters X"",
            ""sex"": ""Mann"",
            ""group"": ""Gruppe des Mitarbeiters X"",
            ""category"": ""Leiter"",
            ""skills"": ""Ein erfahrener leitender Mitarbeiter, der für seine Gruppe verantwortlich ist"",
            ""personality"": ""Er ist eine ernste und verantwortungsbewusste Person, die versucht, mit Respekt zu führen. In seiner Freizeit fährt er gerne Fahrrad.""
        },
        { 
            ""name"": ""Name der Mitarbeiterin Y"",
            ""sex"": ""Frau"",
            ""group"": ""Gruppe der Mitarbeiterin Y"",
            ""category"": ""Senior"",
            ""skills"": ""Eine erfahrene Mitarbeiterin, die mit ihrem Wissen zum Erfolg der Gruppe beiträgt"",
            ""personality"": ""Sie ist eine entspannte Person, die gerne in ihrem eigenen Tempo arbeitet. Sie ist keine große Teamspielerin. In ihrer Freizeit genießt sie Kino und Theater.""
        },
        { 
            ""name"": ""Name des Mitarbeiters Z"",
            ""sex"": ""Mann"",
            ""group"": ""Gruppe der Mitarbeiterin Y"",
            ""category"": ""Normal"",
            ""skills"": ""Ein normaler Mitarbeiter mit nur wenigen Jahren Berufserfahrung"",
            ""personality"": ""Er ist eine ängstliche Person, die sich um die Qualität seiner Arbeit sorgt. Er versucht, die Anweisungen des Leiters so gut wie möglich zu befolgen. In seiner Freizeit spielt er gerne Videospiele.""
        },
        { 
            ""name"": ""Name des Kunden"",
            ""sex"": ""Frau"",
            ""group"": ""Kunden"",
            ""category"": ""Normal"",
            ""skills"": ""Eine Kundin, die möchte, dass das Team ihr angefordertes Projekt entwickelt"",
            ""personality"": ""Sie ist eine anspruchsvolle Person, die der Welt beweisen möchte, wie gut sie ist. In ihrer Freizeit liebt sie Oper und teure Restaurants.""
        }
    ]
}";

    }
}