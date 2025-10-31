namespace yourvrexperience.WorkDay
{
    [System.Serializable]
    public class JSONDataSpanish
    {
        // RESUMEN DEL DOCUMENTO
        public const string documentSummaryJsonString = @"
        {
            ""name"": ""Nombre del documento"",
            ""type"": ""Tipo de documento (texto, imagen)"",
            ""description"": ""Resumen del documento en menos de 200 palabras""
        }";

        // RESPUESTA A REUNIÓN
        public const string replyMeetingJsonString = @"
        {
            ""participant"": ""Nombre del participante de la reunión que da la respuesta"",
            ""reply"": ""Texto de la respuesta del participante de la reunión"",
            ""end"": ""Valor (0,1): 0 la reunión debe continuar, 1 la reunión puede finalizar ya que los temas principales han sido discutidos""
        }";

        public const string documentMeetingJsonString = @"
        {
            ""name"": ""Nombre del documento"",
            ""type"": ""Tipo de documento (texto, imagen)"",
            ""data"": ""Datos en forma de descripción técnica detallada, como texto técnico, código o una descripción textual de una imagen que muestre esos datos detallados""
        }";

        // BASE DE DATOS
        public const string summaryMeetingJsonString = @"[
        {
            ""summary"": ""Resumen detallado de la reunión"",
            ""documents"": [
                { 
                    ""name"": ""Nombre del documento A"",
                    ""person"": ""John"",
                    ""dependency"": """",
                    ""type"": ""requerimientos"",
                    ""time"": ""1"",
                    ""data"": ""Descripción de los requerimientos que debe tener la funcionalidad según las conclusiones de la reunión""
                },
                { 
                    ""name"": ""Nombre del documento B"",
                    ""person"": ""Cathy, Tom"",
                    ""dependency"": ""Nombre del documento A"",
                    ""type"": ""diseño"",
                    ""time"": ""3"",
                    ""data"": ""Descripción del diseño en el que debe basarse la funcionalidad según las conclusiones de la reunión""
                },
                { 
                    ""name"": ""Nombre del documento C"",
                    ""person"": ""Jonas, Steve, Betty"",
                    ""dependency"": ""Nombre del documento B"",
                    ""type"": ""código"",
                    ""time"": ""5"",
                    ""data"": ""Descripción del código que debe implementar la funcionalidad según las conclusiones de la reunión""
                },
                { 
                    ""name"": ""Nombre del documento D"",
                    ""person"": ""Alan, Jennifer"",
                    ""dependency"": ""Nombre del documento C"",
                    ""type"": ""pruebas"",
                    ""time"": ""2"",
                    ""data"": ""Descripción de los pasos de prueba que deben realizarse para verificar el resultado según las conclusiones de la reunión""
                }
            ]
        }
        ]";

        // RESUMEN DE TAREAS
        public const string summaryTasksJsonString = @"[
        {
            ""documents"": [
                { 
                    ""name"": ""Nombre del documento A"",
                    ""person"": ""John"",
                    ""dependency"": """",
                    ""type"": ""requerimientos"",
                    ""time"": ""1"",
                    ""data"": ""Descripción de los requerimientos que debe tener la funcionalidad según la descripción de la tarea""
                },
                { 
                    ""name"": ""Nombre del documento B"",
                    ""person"": ""Cathy, Tom"",
                    ""dependency"": ""Nombre del documento A"",
                    ""type"": ""diseño"",
                    ""time"": ""3"",
                    ""data"": ""Descripción del diseño en el que debe basarse la funcionalidad según la descripción de la tarea""
                },
                { 
                    ""name"": ""Nombre del documento C"",
                    ""person"": ""Jonas, Steve, Betty"",
                    ""dependency"": ""Nombre del documento B"",
                    ""type"": ""código"",
                    ""time"": ""5"",
                    ""data"": ""Descripción del código que debe implementar la funcionalidad según la descripción de la tarea""
                },
                { 
                    ""name"": ""Nombre del documento D"",
                    ""person"": ""Alan, Jennifer"",
                    ""dependency"": ""Nombre del documento C"",
                    ""type"": ""pruebas"",
                    ""time"": ""2"",
                    ""data"": ""Descripción de los pasos de prueba que deben realizarse para verificar el resultado según la descripción de la tarea""
                }
            ]
        }
        ]";

        // DOCUMENTO GENERADO AUTOMÁTICAMENTE
        public const string documentTextGeneratedJsonString = @"
        {
            ""name"": ""Nombre del documento"",
            ""type"": ""Tipo de documento (texto, código)"",
            ""data"": ""Definición detallada del documento basada en la información proporcionada""
        }";

        // DOCUMENTOS GLOBALES
        public const string globalDocumentsJsonString = @"[
            {
                ""name"": ""Documento A"",
                ""tasks"": ""tarea 1, tarea 2""
            },
            {
                ""name"": ""Documento B"",
                ""tasks"": ""tarea 3""
            }
        ]";

        // DESCRIPCIÓN DE FUNCIONALIDADES
        public const string featureDescriptionJsonString = @"[
            {
                ""name"": ""Funcionalidad A"",
                ""description"": ""Descripción detallada de la funcionalidad A que debe implementarse en el próximo sprint del proyecto""
            },
            {
                ""name"": ""Funcionalidad B"",
                ""description"": ""Descripción detallada de la funcionalidad B que debe implementarse en el próximo sprint del proyecto""
            },
            {
                ""name"": ""Funcionalidad C"",
                ""description"": ""Descripción detallada de la funcionalidad C que debe implementarse en el próximo sprint del proyecto""
            }
        ]";

        // DEFINICIÓN DE TAREAS DEL SPRINT
        public const string definitionTasksSprintJsonString = @"[
        {
            ""name"": ""Nombre del tablero del sprint"",
            ""tasks"": [
                { 
                    ""name"": ""Requerimientos del sistema de inicio de sesión"",
                    ""employees"": ""John, Peter"",
                    ""dependency"": """",
                    ""type"": ""requerimientos"",
                    ""time"": ""1"",
                    ""data"": ""Descripción textual, con al menos 150 palabras, de los objetivos de la tarea para definir los requerimientos del sistema de inicio de sesión""
                },
                { 
                    ""name"": ""Diseño de la pantalla de inicio de sesión"",
                    ""person"": ""Cathy, James"",
                    ""dependency"": ""Requerimientos del sistema de inicio de sesión"",
                    ""type"": ""diseño"",
                    ""time"": ""3"",
                    ""data"": ""Descripción textual, con al menos 150 palabras, del objetivo de la tarea para diseñar los elementos de la pantalla de inicio de sesión""
                },
                { 
                    ""name"": ""Programación del sistema de inicio de sesión"",
                    ""person"": ""Jonas, Steve, Betty"",
                    ""dependency"": ""Requerimientos del sistema de inicio de sesión"",
                    ""type"": ""código"",
                    ""time"": ""5"",
                    ""data"": ""Descripción textual, con al menos 150 palabras, del objetivo de la tarea para programar el sistema de inicio de sesión""
                },
                { 
                    ""name"": ""Pruebas del sistema de inicio de sesión"",
                    ""person"": ""Harry"",
                    ""dependency"": ""Programación del sistema de inicio de sesión"",
                    ""type"": ""pruebas"",
                    ""time"": ""2"",
                    ""data"": ""Descripción textual, con al menos 150 palabras, del objetivo de la tarea para probar el sistema de inicio de sesión""
                }
            ]
        }
        ]";

        // DEFINICIÓN DEL TABLERO DEL SPRINT
        public const string documentTextSprintBoarDefinition = @"
        {
            ""name"": ""Nombre del sprint"",
            ""description"": ""Descripción del sprint del proyecto""
        }";

        // DEFINICIÓN DEL PROYECTO
        public const string documentTextProjectDefinition = @"
        {
            ""name"": ""Nombre del proyecto"",
            ""description"": ""Descripción del proyecto""
        }";

        // REUNIONES PARA LAS TAREAS
        public const string meetingForTaskJsonString = @"[
            {
                ""name"": ""Definición de requerimientos para la Tarea A"",
                ""description"": ""Los líderes de Producción, Diseño y Programación se reúnen para definir los requerimientos del proyecto"",
                ""task"": ""Tarea A"",
                ""time"": ""60"",
                ""persons"": ""Cathy, James, Tom, Steve""
            },
            {
                ""name"": ""Implementación de diseño para la Tarea A"",
                ""description"": ""El equipo de Diseño se reúne con los líderes de Producción y Programación para establecer el marco de trabajo y crear un diseño válido"",
                ""task"": ""Tarea A"",
                ""time"": ""90"",
                ""persons"": ""Jonas, Christopher, Robin, Sophia""
            },
            {
                ""name"": ""Implementación de código para la Tarea A"",
                ""description"": ""El equipo de Programación se reúne para organizar cómo implementar la funcionalidad basándose en el diseño proporcionado"",
                ""task"": ""Tarea A"",
                ""time"": ""60"",
                ""persons"": ""Bill, Peter, David, Jil""
            }
        ]";

        // EQUIPO DE LA EMPRESA
        public const string teamCompanyJsonString = @"
        {
            ""projectname"": ""Nombre de un posible proyecto que puede ser desarrollado por esta empresa"",
            ""projectdescription"": ""Descripción del posible proyecto que puede ser desarrollado por esta empresa"",
            ""groups"": [
                { 
                    ""name"": ""Grupo A"",
                    ""description"": ""El grupo A se encargará de una parte del proyecto""
                },
                { 
                    ""name"": ""Grupo B"",
                    ""description"": ""El grupo B se encargará de otra parte del proyecto""
                },
                { 
                    ""name"": ""Grupo C"",
                    ""description"": ""El grupo C se encargará de otra parte adicional del proyecto""
                },
                { 
                    ""name"": ""Clientes"",
                    ""description"": ""El grupo de clientes representa a los clientes de la empresa""
                }
            ],
            ""employees"": [
                { 
                    ""name"": ""Nombre del empleado X"",
                    ""sex"": ""hombre"",
                    ""group"": ""Grupo del empleado X"",
                    ""category"": ""Líder"",
                    ""skills"": ""Un empleado con experiencia que será responsable de su grupo"",
                    ""personality"": ""Es una persona seria y responsable. Intenta liderar con respeto. En su tiempo libre disfruta andar en bicicleta.""
                },
                { 
                    ""name"": ""Nombre de la empleada Y"",
                    ""sex"": ""mujer"",
                    ""group"": ""Grupo de la empleada Y"",
                    ""category"": ""Senior"",
                    ""skills"": ""Una empleada senior que aportará experiencia a su grupo"",
                    ""personality"": ""Es una persona relajada que prefiere trabajar a su propio ritmo. No es muy aficionada al trabajo en equipo. En su tiempo libre disfruta del cine y el teatro.""
                },
                { 
                    ""name"": ""Nombre del empleado Z"",
                    ""sex"": ""hombre"",
                    ""group"": ""Grupo de la empleada Y"",
                    ""category"": ""Normal"",
                    ""skills"": ""Un empleado con pocos años de experiencia"",
                    ""personality"": ""Es una persona ansiosa que se preocupa por la calidad de su trabajo. Intenta seguir las órdenes de su líder lo mejor posible. En su tiempo libre disfruta de los videojuegos.""
                },
                { 
                    ""name"": ""Nombre de la clienta"",
                    ""sex"": ""mujer"",
                    ""group"": ""Clientes"",
                    ""category"": ""Normal"",
                    ""skills"": ""Una clienta que desea que el equipo desarrolle el proyecto que ha solicitado"",
                    ""personality"": ""Es una persona exigente que busca demostrar al mundo lo buena que es. En su tiempo libre disfruta de la ópera y los restaurantes caros.""
                }
            ]
        }";

    }
}