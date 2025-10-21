workspace "Email Summarizer AI" "C4 Architecture (Levels 1–3, Multi-provider Mail + Preprocessing + Secure Token Handling)" {

    !identifiers hierarchical

    model {
        user = person "Authenticated User" "A user who connects one or more email accounts (Gmail, Outlook, Yahoo) to summarize or analyze messages."

        system = softwareSystem "Email Summarizer AI System" "Processes and summarizes emails from multiple providers using AI models." {

            webApp = container "Web Client" "Provides UI for authentication, provider selection, and summarization management." "Angular / React / Next.js"

            api = container "Backend API" "Handles authentication, provider selection, mail fetching, preprocessing, summarization, and persistence." ".NET 8 Web API" {
                
                # Controllers
                authController = component "AuthController" "Manages OAuth 2.0 login and token refresh for all supported providers." "ASP.NET Core Controller"
                emailController = component "EmailController" "Receives user requests to fetch, process, and summarize emails." "ASP.NET Core Controller"

                # Core Services
                emailService = component "EmailService" "Main orchestrator coordinating fetching, preprocessing, and summarization." "C# Service"
                aiService = component "AIService" "Handles communication with OpenAI API for summarization and analysis." "C# Service"

                # Provider Factory & Adapters
                emailProviderFactory = component "EmailProviderFactory" "Resolves provider adapter based on account type (Gmail, Outlook, Yahoo)." "Factory Pattern"
                gmailProvider = component "GmailProvider" "Fetches Gmail data and normalizes it to common schema via Gmail API." "C# Adapter"
                outlookProvider = component "OutlookProvider" "Fetches Outlook data using Microsoft Graph API and normalizes format." "C# Adapter"
                yahooProvider = component "YahooProvider" "Fetches Yahoo Mail data (API/IMAP) and normalizes content." "C# Adapter"

                # Preprocessing pipeline
                emailParser = component "EmailParser" "Parses raw email payload and extracts meaningful content." "C# Service"
                emailCleaner = component "EmailCleaner" "Removes redundant parts (signatures, threads, ads) for summarization input." "C# Service"

                # Token & Security
                credentialEncryptor = component "CredentialEncryptor" "Encrypts/decrypts access tokens before saving to DB." "C# Utility"

                # Data Access Layer
                connectedAccountRepository = component "ConnectedAccountRepository" "Manages linked accounts and encrypted tokens per user." "Repository Pattern"
                emailRepository = component "EmailRepository" "Stores raw and summarized emails." "Repository Pattern"
                appDbContext = component "AppDbContext" "Entity Framework Core database context." "EF Core"
            }

            db = container "SQL Server Database" "Stores email summaries, metadata, and encrypted OAuth tokens." "SQL Server"
            aiApi = container "OpenAI API" "External AI summarization and analysis API." "OpenAI SDK / REST"
        }

        mailApis = softwareSystem "Mail Provider APIs" "External APIs for Gmail, Outlook, and Yahoo email services."
        oauthProviders = softwareSystem "OAuth 2.0 Providers" "External authentication and authorization services (Google, Microsoft, Yahoo)."

        # Relationships
        user -> system.webApp "Logs in and interacts via browser (HTTPS)"
        system.webApp -> oauthProviders "Initiates OAuth 2.0 login per provider"
        oauthProviders -> system.webApp "Returns OAuth tokens"
        system.webApp -> system.api "Sends summarization and account requests"

        system.api -> mailApis "Fetches raw email data using provider APIs"
        system.api -> system.aiApi "Sends cleaned text for AI summarization"
        system.api -> system.db "Reads/Writes summaries, tokens, and metadata"

        # Internal relationships (inside Backend API)
        system.api.authController -> oauthProviders "Handles OAuth 2.0 flow"
        system.api.authController -> system.api.credentialEncryptor "Encrypts tokens before storing"
        system.api.authController -> system.api.connectedAccountRepository "Stores encrypted credentials"
        
        system.api.emailController -> system.api.emailService "Triggers email summarization workflow"
        system.api.emailService -> system.api.emailProviderFactory "Selects provider adapter"
        system.api.emailProviderFactory -> system.api.gmailProvider "Uses Gmail adapter"
        system.api.emailProviderFactory -> system.api.outlookProvider "Uses Outlook adapter"
        system.api.emailProviderFactory -> system.api.yahooProvider "Uses Yahoo adapter"
        
        system.api.emailService -> system.api.emailParser "Extracts email content"
        system.api.emailService -> system.api.emailCleaner "Cleans redundant content"
        system.api.emailService -> system.api.aiService "Sends cleaned text for summarization"
        system.api.aiService -> system.aiApi "Calls OpenAI API (REST)"
        system.api.emailService -> system.api.emailRepository "Saves summarized data"
        system.api.emailService -> system.api.connectedAccountRepository "Retrieves OAuth tokens"
        system.api.emailRepository -> system.api.appDbContext "Uses EF Core"
        system.api.connectedAccountRepository -> system.api.appDbContext "Uses EF Core"
        system.api.appDbContext -> system.db "Persists data"
    }

    views {
        systemContext system "SystemContext" {
            include *
            autolayout lr
            title "Level 1 - System Context (Multi-Provider + OAuth 2.0)"
        }

        container system "ContainerDiagram" {
            include *
            autolayout lr
            title "Level 2 - Container Diagram (Backend + Multi-Provider Layer)"
        }

        component system.api "ComponentDiagram" {
            include *
            autolayout lr
            title "Level 3 - Backend API Components (Preprocessing + Secure Tokens)"
        }

        styles {
            element "Person" {
                shape person
                background #9333ea
                color #ffffff
            }
            element "Software System" {
                background #2563eb
                color #ffffff
            }
            element "Container" {
                background #3b82f6
                color #ffffff
            }
            element "Component" {
                background #60a5fa
                color #ffffff
            }
            element "Database" {
                shape cylinder
                background #1e3a8a
                color #ffffff
            }
            relationship "Relationship" {
                thickness 3
                color #555555
            }
        }
    }
}
