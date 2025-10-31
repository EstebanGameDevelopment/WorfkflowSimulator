namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataCatalan
    {
        // RESUM DEL DOCUMENT
        public const string documentSummaryJsonString = @"
{
    ""name"": ""Nom del document"",
    ""type"": ""Tipus de document (text, imatge)"",
    ""description"": ""Resum del document en menys de 200 paraules""
}";

        // RESPOSTA DE REUNIÓ
        public const string replyMeetingJsonString = @"
{
    ""participant"": ""Nom del participant de la reunió que fa la resposta"",
    ""reply"": ""Text de la resposta del participant de la reunió"",
    ""end"": ""Valor (0,1): 0 la reunió ha de continuar, 1 la reunió pot finalitzar ja que s’han tractat els temes principals""
}";

        public const string documentMeetingJsonString = @"
{
    ""name"": ""Nom del document"",
    ""type"": ""Tipus de document (text, imatge)"",
    ""data"": ""Dades en forma de descripció tècnica detallada, com ara text tècnic, codi o una descripció textual d’una imatge que mostri aquestes dades detallades""
}";

        // BASE DE DADES
        public const string summaryMeetingJsonString = @"[
{
    ""summary"": ""Resum detallat de la reunió"",
    ""documents"": [
        { 
            ""name"": ""Nom del document A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""requeriments"",
            ""time"": ""1"",
            ""data"": ""Descripció dels requeriments que ha de tenir la funcionalitat segons les conclusions de la reunió""
        },
        { 
            ""name"": ""Nom del document B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nom del document A"",
            ""type"": ""disseny"",
            ""time"": ""3"",
            ""data"": ""Descripció del disseny en què s’ha de basar la funcionalitat segons les conclusions de la reunió""
        },
        { 
            ""name"": ""Nom del document C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nom del document B"",
            ""type"": ""codi"",
            ""time"": ""5"",
            ""data"": ""Descripció del codi que ha d’implementar la funcionalitat segons les conclusions de la reunió""
        },
        { 
            ""name"": ""Nom del document D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nom del document C"",
            ""type"": ""proves"",
            ""time"": ""2"",
            ""data"": ""Descripció dels passos de prova que s’han de fer per verificar el resultat segons les conclusions de la reunió""
        }
    ]
}
]";

        public const string summaryTasksJsonString = @"[
{
    ""documents"": [
        { 
            ""name"": ""Nom del document A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""requeriments"",
            ""time"": ""1"",
            ""data"": ""Descripció dels requeriments que ha de tenir la funcionalitat segons la descripció de la tasca""
        },
        { 
            ""name"": ""Nom del document B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nom del document A"",
            ""type"": ""disseny"",
            ""time"": ""3"",
            ""data"": ""Descripció del disseny en què s’ha de basar la funcionalitat segons la descripció de la tasca""
        },
        { 
            ""name"": ""Nom del document C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nom del document B"",
            ""type"": ""codi"",
            ""time"": ""5"",
            ""data"": ""Descripció del codi que ha d’implementar la funcionalitat segons la descripció de la tasca""
        },
        { 
            ""name"": ""Nom del document D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nom del document C"",
            ""type"": ""proves"",
            ""time"": ""2"",
            ""data"": ""Descripció dels passos de prova que s’han de fer per verificar el resultat segons la descripció de la tasca""
        }
    ]
}
]";

        public const string documentTextGeneratedJsonString = @"
{
    ""name"": ""Nom del document"",
    ""type"": ""Tipus de document (text, codi)"",
    ""data"": ""Definició detallada del document basada en la informació proporcionada""
}";

        public const string globalDocumentsJsonString = @"[
{
    ""name"": ""Document A"",
    ""tasks"": ""tasca 1, tasca 2""
},
{
    ""name"": ""Document B"",
    ""tasks"": ""tasca 3""
}
]";

        public const string featureDescriptionJsonString = @"[
{
    ""name"": ""Funcionalitat A"",
    ""description"": ""Descripció detallada de la funcionalitat A que s’ha d’implementar en el proper sprint del projecte""
},
{
    ""name"": ""Funcionalitat B"",
    ""description"": ""Descripció detallada de la funcionalitat B que s’ha d’implementar en el proper sprint del projecte""
},
{
    ""name"": ""Funcionalitat C"",
    ""description"": ""Descripció detallada de la funcionalitat C que s’ha d’implementar en el proper sprint del projecte""
}
]";

        public const string definitionTasksSprintJsonString = @"[
{
    ""name"": ""Nom del tauler de sprint"",
    ""tasks"": [
        { 
            ""name"": ""Requeriments del sistema d’inici de sessió"",
            ""employees"": ""John, Peter"",
            ""dependency"": """",
            ""type"": ""requeriments"",
            ""time"": ""1"",
            ""data"": ""Descripció textual, d’almenys 150 paraules, dels objectius de la tasca per definir els requeriments del sistema d’inici de sessió""
        },
        { 
            ""name"": ""Disseny de la pantalla d’inici de sessió"",
            ""person"": ""Cathy, James"",
            ""dependency"": ""Requeriments del sistema d’inici de sessió"",
            ""type"": ""disseny"",
            ""time"": ""3"",
            ""data"": ""Descripció textual, d’almenys 150 paraules, de l’objectiu de la tasca per dissenyar els elements de la pantalla d’inici de sessió""
        },
        { 
            ""name"": ""Programació del sistema d’inici de sessió"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Requeriments del sistema d’inici de sessió"",
            ""type"": ""codi"",
            ""time"": ""5"",
            ""data"": ""Descripció textual, d’almenys 150 paraules, de l’objectiu de la tasca per programar el sistema d’inici de sessió""
        },
        { 
            ""name"": ""Proves del sistema d’inici de sessió"",
            ""person"": ""Harry"",
            ""dependency"": ""Programació del sistema d’inici de sessió"",
            ""type"": ""proves"",
            ""time"": ""2"",
            ""data"": ""Descripció textual, d’almenys 150 paraules, de l’objectiu de la tasca per provar el sistema d’inici de sessió""
        }
    ]
}
]";

        public const string documentTextSprintBoarDefinition = @"
{
    ""name"": ""Nom de l’sprint"",
    ""description"": ""Descripció de l’sprint del projecte""
}";

        public const string documentTextProjectDefinition = @"
{
    ""name"": ""Nom del projecte"",
    ""description"": ""Descripció del projecte""
}";

        public const string meetingForTaskJsonString = @"[
{
    ""name"": ""Definició de requeriments per a la tasca A"",
    ""description"": ""Els responsables de Producció, Disseny i Programació es reuneixen per definir els requeriments del projecte"",
    ""task"": ""Tasca A"",
    ""time"": ""60"",
    ""persons"": ""Cathy, James, Tom, Steve""
},
{
    ""name"": ""Implementació del disseny per a la tasca A"",
    ""description"": ""L’equip de Disseny es reuneix amb els responsables dels equips de Producció i Programació per establir el marc de treball per crear un disseny vàlid"",
    ""task"": ""Tasca A"",
    ""time"": ""90"",
    ""persons"": ""Jonas, Christopher, Robin, Sophia""
},
{
    ""name"": ""Implementació del codi per a la tasca A"",
    ""description"": ""L’equip de Programació es reuneix per organitzar com implementar la funcionalitat amb el disseny proporcionat"",
    ""task"": ""Tasca A"",
    ""time"": ""60"",
    ""persons"": ""Bill, Peter, David, Jil""
}
]";

        public const string teamCompanyJsonString = @"
{
    ""projectname"": ""Nom d’un possible projecte que pot ser desenvolupat per aquesta empresa"",
    ""projectdescription"": ""Descripció del possible projecte que pot ser desenvolupat per aquesta empresa"",
    ""groups"": [
        { 
            ""name"": ""Grup A"",
            ""description"": ""El grup A s’encarregarà d’una part del projecte""
        },
        { 
            ""name"": ""Grup B"",
            ""description"": ""El grup B s’encarregarà d’una altra part del projecte""
        },
        { 
            ""name"": ""Grup C"",
            ""description"": ""El grup C s’encarregarà d’una altra part addicional del projecte""
        },
        { 
            ""name"": ""Clients"",
            ""description"": ""El grup de clients representa els clients de l’empresa""
        }
    ],
    ""employees"": [
        { 
            ""name"": ""Nom de l’empleat X"",
            ""sex"": ""home"",
            ""group"": ""Grup de l’empleat X"",
            ""category"": ""Líder"",
            ""skills"": ""Un empleat amb experiència que serà responsable del seu grup"",
            ""personality"": ""És una persona seriosa i responsable. Intenta dirigir amb respecte. En el seu temps lliure li agrada anar amb bicicleta.""
        },
        { 
            ""name"": ""Nom de l’empleada Y"",
            ""sex"": ""dona"",
            ""group"": ""Grup de l’empleada Y"",
            ""category"": ""Sènior"",
            ""skills"": ""Una empleada sènior que contribuirà al seu grup amb la seva experiència"",
            ""personality"": ""És una persona tranquil·la que li agrada treballar al seu ritme. No és gaire treballadora en equip. En el seu temps lliure gaudeix del cinema i el teatre.""
        },
        { 
            ""name"": ""Nom de l’empleat Z"",
            ""sex"": ""home"",
            ""group"": ""Grup de l’empleada Y"",
            ""category"": ""Normal"",
            ""skills"": ""Un empleat amb pocs anys d’experiència"",
            ""personality"": ""És una persona ansiosa que es preocupa per la qualitat de la feina que fa. Intenta seguir les ordres del líder tan bé com pot. En el seu temps lliure gaudeix dels videojocs.""
        },
        { 
            ""name"": ""Nom de la clienta"",
            ""sex"": ""dona"",
            ""group"": ""Clients"",
            ""category"": ""Normal"",
            ""skills"": ""Una clienta que vol que l’equip desenvolupi el projecte que ha sol·licitat"",
            ""personality"": ""És una persona exigent que intenta demostrar al món com de bona és. En el seu temps lliure li agrada l’òpera i els restaurants cars.""
        }
    ]
}";

    }
}