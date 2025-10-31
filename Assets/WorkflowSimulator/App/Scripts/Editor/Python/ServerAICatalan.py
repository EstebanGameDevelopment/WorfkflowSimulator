from ai_endpoints.AILLMEndpoints import AILLMServer
from ai_endpoints.CatalanInstructions import InstructionsAI
import argparse

# We need JSON prompts for each language
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

instructions_ai = InstructionsAI()
            
# ************************************
# ************************************
# OLLAMA SERVER ENDPOINTS
# ************************************
# ************************************

if __name__ == '__main__':
    # Create the argument parser
    parser = argparse.ArgumentParser(description="Start the AI LLM Server.")
    parser.add_argument(
        '--port', type=int, default=5007, help='Port number for the server (default: 5000)'
    )
    
    # Parse the arguments
    args = parser.parse_args()
    
    ai_llm_server = AILLMServer('0.0.0.0',
                            args.port,
                            instructions_ai.databaseAlchemy, 
                            instructions_ai.voicesLanguage, 
                            instructions_ai.urlSpeechGeneration,
                            instructions_ai.urlImageGeneration,
                            instructions_ai.urlFluxImageGeneration,
                            instructions_ai.templateQuestion,
                            instructions_ai.promptChapters,
                            instructions_ai.parserChapters,
                            instructions_ai.promptDocumentSummary,
                            instructions_ai.parserDocumentSummary,
                            instructions_ai.promptMeetingReply,
                            instructions_ai.parserMeetingReply,
                            instructions_ai.promptMeetingSummary,
                            instructions_ai.parserMeetingSummary,
                            instructions_ai.promptTasksDocumentsTODO,
                            instructions_ai.parserTasksDocumentsTODO,
                            instructions_ai.promptDocumentGeneration,
                            instructions_ai.parserDocumentGeneration,
                            instructions_ai.promptMakeDocumentsGlobal,
                            instructions_ai.parserMakeDocumentsGlobal,
                            instructions_ai.promptFeatureDescription,
                            instructions_ai.parserFeatureDescription,                            
                            instructions_ai.promptTasksForSprint,
                            instructions_ai.parserTasksForSprint,
                            instructions_ai.promptBoardSprintDefinition,
                            instructions_ai.parserBoardSprintDefinition,
                            instructions_ai.promptProjectDefinition,
                            instructions_ai.parserProjectDefinition,
                            instructions_ai.promptMeetingForTask,
                            instructions_ai.parserMeetingForTask,                            
                            instructions_ai.promptTeamCompany,
                            instructions_ai.parserTeamCompany)
    ai_llm_server.start_webserver()
