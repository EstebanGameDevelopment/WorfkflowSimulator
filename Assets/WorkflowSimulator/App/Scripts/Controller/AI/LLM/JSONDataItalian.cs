namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataItalian
    {
        // DOCUMENT SUMMARY
        public const string documentSummaryJsonString = @"
{
    ""name"": ""Nome del documento"",
    ""type"": ""Tipo di documento (testo, immagine)"",
    ""description"": ""Riassunto del documento in meno di 200 parole""
}";

        // REPLY MEETING
        public const string replyMeetingJsonString = @"
{
    ""participant"": ""Nome del partecipante alla riunione che risponde"",
    ""reply"": ""Testo della risposta del partecipante alla riunione"",
    ""end"": ""Valore (0,1): 0 la riunione deve continuare, 1 la riunione può terminare poiché gli argomenti principali sono stati discussi""
}";

        public const string documentMeetingJsonString = @"
{
    ""name"": ""Nome del documento"",
    ""type"": ""Tipo di documento (testo, immagine)"",
    ""data"": ""Dati sotto forma di descrizione tecnica dettagliata, come testo tecnico, codice o descrizione testuale di un’immagine che rappresenta tali dati dettagliati""
}";

        // DATABASE
        public const string summaryMeetingJsonString = @"[
{
    ""summary"": ""Riassunto dettagliato della riunione"",
    ""documents"": [
        { 
            ""name"": ""Nome del documento A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""requisiti"",
            ""time"": ""1"",
            ""data"": ""Descrizione dei requisiti che la funzionalità deve avere in base alle conclusioni della riunione""
        },
        { 
            ""name"": ""Nome del documento B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nome del documento A"",
            ""type"": ""progetto"",
            ""time"": ""3"",
            ""data"": ""Descrizione del progetto su cui deve basarsi la funzionalità secondo le conclusioni della riunione""
        },
        { 
            ""name"": ""Nome del documento C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nome del documento B"",
            ""type"": ""codice"",
            ""time"": ""5"",
            ""data"": ""Descrizione del codice che la funzionalità deve implementare in base alle conclusioni della riunione""
        },
        { 
            ""name"": ""Nome del documento D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nome del documento C"",
            ""type"": ""test"",
            ""time"": ""2"",
            ""data"": ""Descrizione dei passaggi di test da eseguire per verificare il risultato secondo le conclusioni della riunione""
        }
    ]
}
]";

        public const string summaryTasksJsonString = @"[
{
    ""documents"": [
        { 
            ""name"": ""Nome del documento A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""requisiti"",
            ""time"": ""1"",
            ""data"": ""Descrizione dei requisiti che la funzionalità deve avere in base alla descrizione dell’attività""
        },
        { 
            ""name"": ""Nome del documento B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nome del documento A"",
            ""type"": ""progetto"",
            ""time"": ""3"",
            ""data"": ""Descrizione del progetto su cui la funzionalità deve basarsi in base alla descrizione dell’attività""
        },
        { 
            ""name"": ""Nome del documento C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nome del documento B"",
            ""type"": ""codice"",
            ""time"": ""5"",
            ""data"": ""Descrizione del codice che la funzionalità deve implementare in base alla descrizione dell’attività""
        },
        { 
            ""name"": ""Nome del documento D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nome del documento C"",
            ""type"": ""test"",
            ""time"": ""2"",
            ""data"": ""Descrizione dei passaggi di test da eseguire per verificare il risultato in base alla descrizione dell’attività""
        }
    ]
}
]";

        public const string documentTextGeneratedJsonString = @"
{
    ""name"": ""Nome del documento"",
    ""type"": ""Tipo di documento (testo, codice)"",
    ""data"": ""Definizione dettagliata del documento basata sulle informazioni fornite""
}";

        public const string globalDocumentsJsonString = @"[
{
    ""name"": ""Documento A"",
    ""tasks"": ""attività 1, attività 2""
},
{
    ""name"": ""Documento B"",
    ""tasks"": ""attività 3""
}
]";

        public const string featureDescriptionJsonString = @"[
{
    ""name"": ""Funzionalità A"",
    ""description"": ""Descrizione dettagliata della funzionalità A da implementare nel prossimo sprint del progetto""
},
{
    ""name"": ""Funzionalità B"",
    ""description"": ""Descrizione dettagliata della funzionalità B da implementare nel prossimo sprint del progetto""
},
{
    ""name"": ""Funzionalità C"",
    ""description"": ""Descrizione dettagliata della funzionalità C da implementare nel prossimo sprint del progetto""
}
]";

        public const string definitionTasksSprintJsonString = @"[
{
    ""name"": ""Nome della board dello sprint"",
    ""tasks"": [
        { 
            ""name"": ""Requisiti del sistema di login"",
            ""employees"": ""John, Peter"",
            ""dependency"": """",
            ""type"": ""requisiti"",
            ""time"": ""1"",
            ""data"": ""Descrizione testuale, di almeno 150 parole, degli obiettivi dell’attività per definire i requisiti del sistema di login""
        },
        { 
            ""name"": ""Progettazione schermata di login"",
            ""person"": ""Cathy, James"",
            ""dependency"": ""Requisiti del sistema di login"",
            ""type"": ""progetto"",
            ""time"": ""3"",
            ""data"": ""Descrizione testuale, di almeno 150 parole, dell’obiettivo dell’attività per progettare gli elementi della schermata di login""
        },
        { 
            ""name"": ""Programmazione del sistema di login"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Requisiti del sistema di login"",
            ""type"": ""codice"",
            ""time"": ""5"",
            ""data"": ""Descrizione testuale, di almeno 150 parole, dell’obiettivo dell’attività per programmare il sistema di login""
        },
        { 
            ""name"": ""Test del sistema di login"",
            ""person"": ""Harry"",
            ""dependency"": ""Programmazione del sistema di login"",
            ""type"": ""test"",
            ""time"": ""2"",
            ""data"": ""Descrizione testuale, di almeno 150 parole, dell’obiettivo dell’attività per testare il sistema di login""
        }
    ]
}
]";

        public const string documentTextSprintBoarDefinition = @"
{
    ""name"": ""Nome dello sprint"",
    ""description"": ""Descrizione dello sprint del progetto""
}";

        public const string documentTextProjectDefinition = @"
{
    ""name"": ""Nome del progetto"",
    ""description"": ""Descrizione del progetto""
}";

        public const string meetingForTaskJsonString = @"[
{
    ""name"": ""Definizione dei requisiti per l’attività A"",
    ""description"": ""I responsabili della Produzione, del Design e della Programmazione si incontrano per definire i requisiti del progetto"",
    ""task"": ""Attività A"",
    ""time"": ""60"",
    ""persons"": ""Cathy, James, Tom, Steve""
},
{
    ""name"": ""Implementazione del design per l’attività A"",
    ""description"": ""Il team di Design si incontra con i responsabili della Produzione e della Programmazione per stabilire la struttura necessaria a creare un design valido"",
    ""task"": ""Attività A"",
    ""time"": ""90"",
    ""persons"": ""Jonas, Christopher, Robin, Sophia""
},
{
    ""name"": ""Implementazione del codice per l’attività A"",
    ""description"": ""Il team di Programmazione si riunisce per organizzare come implementare la funzionalità in base al design fornito"",
    ""task"": ""Attività A"",
    ""time"": ""60"",
    ""persons"": ""Bill, Peter, David, Jil""
}
]";

        public const string teamCompanyJsonString = @"
{
    ""projectname"": ""Nome di un possibile progetto che può essere sviluppato da questa azienda"",
    ""projectdescription"": ""Descrizione del possibile progetto che può essere sviluppato da questa azienda"",
    ""groups"": [
        { 
            ""name"": ""Gruppo A"",
            ""description"": ""Il gruppo A si occuperà di una parte del progetto""
        },
        { 
            ""name"": ""Gruppo B"",
            ""description"": ""Il gruppo B si occuperà di un’altra parte del progetto""
        },
        { 
            ""name"": ""Gruppo C"",
            ""description"": ""Il gruppo C si occuperà di un’ulteriore parte del progetto""
        },
        { 
            ""name"": ""Clienti"",
            ""description"": ""Il gruppo dei clienti rappresenta i clienti dell’azienda""
        }
    ],
    ""employees"": [
        { 
            ""name"": ""Nome del dipendente X"",
            ""sex"": ""uomo"",
            ""group"": ""Gruppo del dipendente X"",
            ""category"": ""Lead"",
            ""skills"": ""Un dipendente esperto che sarà responsabile del proprio gruppo"",
            ""personality"": ""È una persona seria e responsabile. Cerca di guidare con rispetto. Nel tempo libero ama andare in bicicletta.""
        },
        { 
            ""name"": ""Nome del dipendente Y"",
            ""sex"": ""donna"",
            ""group"": ""Gruppo del dipendente Y"",
            ""category"": ""Senior"",
            ""skills"": ""Una dipendente senior che contribuirà al suo gruppo con la propria esperienza"",
            ""personality"": ""È una persona tranquilla che preferisce lavorare con i propri ritmi. Non è molto incline al lavoro di squadra. Nel tempo libero ama il cinema e il teatro.""
        },
        { 
            ""name"": ""Nome del dipendente Z"",
            ""sex"": ""uomo"",
            ""group"": ""Gruppo del dipendente Y"",
            ""category"": ""Normale"",
            ""skills"": ""Un dipendente con solo pochi anni di esperienza"",
            ""personality"": ""È una persona ansiosa, preoccupata per la qualità del proprio lavoro. Cerca di seguire le istruzioni del responsabile nel miglior modo possibile. Nel tempo libero ama i videogiochi.""
        },
        { 
            ""name"": ""Nome del cliente"",
            ""sex"": ""donna"",
            ""group"": ""Clienti"",
            ""category"": ""Normale"",
            ""skills"": ""Una cliente che desidera che il team sviluppi il progetto richiesto"",
            ""personality"": ""È una persona esigente che vuole dimostrare al mondo quanto vale. Nel tempo libero ama l’opera e i ristoranti di lusso.""
        }
    ]
}";
    }
}