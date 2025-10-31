namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataFrench
    {
        // RÉSUMÉ DU DOCUMENT
        public const string documentSummaryJsonString = @"
{
    ""name"": ""Nom du document"",
    ""type"": ""Type de document (texte, image)"",
    ""description"": ""Résumé du document en moins de 200 mots""
}";

        // RÉPONSE À LA RÉUNION
        public const string replyMeetingJsonString = @"
{
    ""participant"": ""Nom du participant à la réunion qui donne la réponse"",
    ""reply"": ""Texte de la réponse du participant à la réunion"",
    ""end"": ""Valeur (0,1) : 0 la réunion doit continuer, 1 la réunion peut se terminer car les principaux sujets ont été abordés""
}";

        public const string documentMeetingJsonString = @"
{
    ""name"": ""Nom du document"",
    ""type"": ""Type de document (texte, image)"",
    ""data"": ""Données sous forme de description technique détaillée, comme un texte technique, du code ou une description textuelle d'une image représentant ces données détaillées""
}";

        // BASE DE DONNÉES
        public const string summaryMeetingJsonString = @"[
{
    ""summary"": ""Résumé détaillé de la réunion"",
    ""documents"": [
        { 
            ""name"": ""Nom du document A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""exigences"",
            ""time"": ""1"",
            ""data"": ""Description des exigences que la fonctionnalité doit avoir selon les conclusions de la réunion""
        },
        { 
            ""name"": ""Nom du document B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nom du document A"",
            ""type"": ""conception"",
            ""time"": ""3"",
            ""data"": ""Description de la conception sur laquelle la fonctionnalité doit être basée selon les conclusions de la réunion""
        },
        { 
            ""name"": ""Nom du document C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nom du document B"",
            ""type"": ""code"",
            ""time"": ""5"",
            ""data"": ""Description du code que la fonctionnalité doit implémenter selon les conclusions de la réunion""
        },
        { 
            ""name"": ""Nom du document D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nom du document C"",
            ""type"": ""tests"",
            ""time"": ""2"",
            ""data"": ""Description des étapes de test à effectuer pour vérifier le résultat selon les conclusions de la réunion""
        }
    ]
}
]";

        // RÉSUMÉ DES TÂCHES
        public const string summaryTasksJsonString = @"[
{
    ""documents"": [
        { 
            ""name"": ""Nom du document A"",
            ""person"": ""John"",
            ""dependency"": """",
            ""type"": ""exigences"",
            ""time"": ""1"",
            ""data"": ""Description des exigences que la fonctionnalité doit avoir selon la description de la tâche""
        },
        { 
            ""name"": ""Nom du document B"",
            ""person"": ""Cathy, Tom"",
            ""dependency"": ""Nom du document A"",
            ""type"": ""conception"",
            ""time"": ""3"",
            ""data"": ""Description de la conception sur laquelle la fonctionnalité doit être basée selon la description de la tâche""
        },
        { 
            ""name"": ""Nom du document C"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Nom du document B"",
            ""type"": ""code"",
            ""time"": ""5"",
            ""data"": ""Description du code que la fonctionnalité doit implémenter selon la description de la tâche""
        },
        { 
            ""name"": ""Nom du document D"",
            ""person"": ""Alan, Jennifer"",
            ""dependency"": ""Nom du document C"",
            ""type"": ""tests"",
            ""time"": ""2"",
            ""data"": ""Description des étapes de test à effectuer pour vérifier le résultat selon la description de la tâche""
        }
    ]
}
]";

        // DOCUMENT TEXTE GÉNÉRÉ
        public const string documentTextGeneratedJsonString = @"
{
    ""name"": ""Nom du document"",
    ""type"": ""Type de document (texte, code)"",
    ""data"": ""Définition détaillée du document basée sur les informations fournies""
}";

        // DOCUMENTS GLOBAUX
        public const string globalDocumentsJsonString = @"[
{
    ""name"": ""Document A"",
    ""tasks"": ""tâche 1, tâche 2""
},
{
    ""name"": ""Document B"",
    ""tasks"": ""tâche 3""
}
]";

        // DESCRIPTION DES FONCTIONNALITÉS
        public const string featureDescriptionJsonString = @"[
{
    ""name"": ""Fonctionnalité A"",
    ""description"": ""Description détaillée de la fonctionnalité A à implémenter dans le prochain sprint du projet""
},
{
    ""name"": ""Fonctionnalité B"",
    ""description"": ""Description détaillée de la fonctionnalité B à implémenter dans le prochain sprint du projet""
},
{
    ""name"": ""Fonctionnalité C"",
    ""description"": ""Description détaillée de la fonctionnalité C à implémenter dans le prochain sprint du projet""
}
]";

        // DÉFINITION DES TÂCHES DU SPRINT
        public const string definitionTasksSprintJsonString = @"[
{
    ""name"": ""Nom du tableau de sprint"",
    ""tasks"": [
        { 
            ""name"": ""Exigences du système de connexion"",
            ""employees"": ""John, Peter"",
            ""dependency"": """",
            ""type"": ""exigences"",
            ""time"": ""1"",
            ""data"": ""Description textuelle, d'au moins 150 mots, des objectifs de la tâche visant à définir les exigences du système de connexion""
        },
        { 
            ""name"": ""Conception de l’écran de connexion"",
            ""person"": ""Cathy, James"",
            ""dependency"": ""Exigences du système de connexion"",
            ""type"": ""conception"",
            ""time"": ""3"",
            ""data"": ""Description textuelle, d'au moins 150 mots, de l’objectif de la tâche visant à concevoir les éléments de l’écran de connexion""
        },
        { 
            ""name"": ""Programmation du système de connexion"",
            ""person"": ""Jonas, Steve, Betty"",
            ""dependency"": ""Exigences du système de connexion"",
            ""type"": ""code"",
            ""time"": ""5"",
            ""data"": ""Description textuelle, d'au moins 150 mots, de l’objectif de la tâche visant à programmer le système de connexion""
        },
        { 
            ""name"": ""Tests du système de connexion"",
            ""person"": ""Harry"",
            ""dependency"": ""Programmation du système de connexion"",
            ""type"": ""tests"",
            ""time"": ""2"",
            ""data"": ""Description textuelle, d'au moins 150 mots, de l’objectif de la tâche visant à tester le système de connexion""
        }
    ]
}
]";

        // DÉFINITION DU TABLEAU DE SPRINT
        public const string documentTextSprintBoarDefinition = @"
{
    ""name"": ""Nom du sprint"",
    ""description"": ""Description du sprint du projet""
}";

        // DÉFINITION DU PROJET
        public const string documentTextProjectDefinition = @"
{
    ""name"": ""Nom du projet"",
    ""description"": ""Description du projet""
}";

        // RÉUNIONS POUR LES TÂCHES
        public const string meetingForTaskJsonString = @"[
{
    ""name"": ""Définition des exigences pour la tâche A"",
    ""description"": ""Les responsables de la production, du design et de la programmation se réunissent pour définir les exigences du projet"",
    ""task"": ""Tâche A"",
    ""time"": ""60"",
    ""persons"": ""Cathy, James, Tom, Steve""
},
{
    ""name"": ""Mise en œuvre du design pour la tâche A"",
    ""description"": ""L’équipe de design se réunit avec les responsables de la production et de la programmation pour établir le cadre nécessaire à la création d’un design valide"",
    ""task"": ""Tâche A"",
    ""time"": ""90"",
    ""persons"": ""Jonas, Christopher, Robin, Sophia""
},
{
    ""name"": ""Mise en œuvre du code pour la tâche A"",
    ""description"": ""L’équipe de programmation se réunit afin d’organiser la mise en œuvre de la fonctionnalité en fonction du design fourni"",
    ""task"": ""Tâche A"",
    ""time"": ""60"",
    ""persons"": ""Bill, Peter, David, Jil""
}
]";

        // ÉQUIPE DE L'ENTREPRISE
        public const string teamCompanyJsonString = @"
{
    ""projectname"": ""Nom d’un projet potentiel pouvant être développé par cette entreprise"",
    ""projectdescription"": ""Description du projet potentiel pouvant être développé par cette entreprise"",
    ""groups"": [
        { 
            ""name"": ""Groupe A"",
            ""description"": ""Le groupe A sera responsable d’une partie du projet""
        },
        { 
            ""name"": ""Groupe B"",
            ""description"": ""Le groupe B sera responsable d’une autre partie du projet""
        },
        { 
            ""name"": ""Groupe C"",
            ""description"": ""Le groupe C prendra en charge une autre partie du projet""
        },
        { 
            ""name"": ""Clients"",
            ""description"": ""Le groupe des clients représente les clients de l’entreprise""
        }
    ],
    ""employees"": [
        { 
            ""name"": ""Nom de l’employé X"",
            ""sex"": ""homme"",
            ""group"": ""Groupe de l’employé X"",
            ""category"": ""Chef d’équipe"",
            ""skills"": ""Un employé expérimenté responsable de son groupe"",
            ""personality"": ""C’est une personne sérieuse et responsable. Il essaie de diriger avec respect. Pendant son temps libre, il aime faire du vélo.""
        },
        { 
            ""name"": ""Nom de l’employée Y"",
            ""sex"": ""femme"",
            ""group"": ""Groupe de l’employée Y"",
            ""category"": ""Senior"",
            ""skills"": ""Une employée senior qui contribue à son groupe grâce à son expérience"",
            ""personality"": ""C’est une personne détendue qui aime travailler à son propre rythme. Elle n’est pas très portée sur le travail d’équipe. Pendant son temps libre, elle apprécie le cinéma et le théâtre.""
        },
        { 
            ""name"": ""Nom de l’employé Z"",
            ""sex"": ""homme"",
            ""group"": ""Groupe de l’employée Y"",
            ""category"": ""Normal"",
            ""skills"": ""Un employé avec seulement quelques années d’expérience"",
            ""personality"": ""C’est une personne anxieuse, soucieuse de la qualité de son travail. Il essaie de suivre les instructions de son chef du mieux qu’il peut. Pendant son temps libre, il aime les jeux vidéo.""
        },
        { 
            ""name"": ""Nom de la cliente"",
            ""sex"": ""femme"",
            ""group"": ""Clients"",
            ""category"": ""Normal"",
            ""skills"": ""Une cliente qui souhaite que l’équipe développe le projet qu’elle a demandé"",
            ""personality"": ""C’est une personne exigeante qui veut prouver sa valeur au monde. Pendant son temps libre, elle aime l’opéra et les restaurants gastronomiques.""
        }
    ]
}";

    }
}