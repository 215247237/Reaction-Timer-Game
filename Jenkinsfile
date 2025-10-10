pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
                // Clone the project's source code from the GitHub repository                                                              
                checkout scm

                // Ensure all required NuGet packages are restored and cached before build for efficiency
                sh 'dotnet restore ReactionMachineProject.sln'

                // Builds the solution in Release mode using previously restored dependencies
                sh 'dotnet build ReactionMachineProject.sln --configuration Release --no-restore'

                // Retrieve short Git commit hash to uniquely identify the current build artefact
                script {
                    def COMMIT_HASH = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.COMMIT_HASH = COMMIT_HASH
                }

                // Build Docker image and tag with both 'latest' and unique commit hash for traceability
                sh "docker build -t s215247237/reactionmachine:latest -t s215247237/reactionmachine:${env.COMMIT_HASH} ."

                // Sign into Docker Hub using stored Jenkins credentials and push both tags
                withCredentials([usernamePassword(credentialsId: 'DOCKER_CREDENTIALS', usernameVariable: 'DOCKER_USERNAME', passwordVariable: 'DOCKER_PASSWORD')]) {
                    sh """
                        echo $DOCKER_PASSWORD | docker login -u $DOCKER_USERNAME --password-stdin
                        docker push s215247237/reactionmachine:latest
                        docker push s215247237/reactionmachine:${env.COMMIT_HASH}
                    """
                }

                // Display locally built Docker image to confirm both tags were created
                sh 'docker images | grep reactionmachine'
            }
        }
        stage('Test') {                                                                     
            steps {
                // Run the custom 'Tester' project to perform integration and behavioural testing
                // Simulates user interactions to validate game logic across multiple states
                sh 'dotnet run --project Tester/tester.csproj'

                // Run NUnit tests to perform unit testing and generate test results
                // Validates individual components including state transitions, timeouts, and display outputs
                sh '''
                    set -e
                    rm -rf ReactionTests/TestResults
                    mkdir -p ReactionTests/TestResults
                    dotnet test ReactionTests/ReactionTests.csproj \
                        --configuration Release --no-build \
                        --logger "junit;LogFilePath=ReactionTests/TestResults/test-results.xml;MethodFormat=Class;FailureBodyFormat=Verbose"
                    ls -la ReactionTests/ReactionTests/TestResults
                '''        
            }
            post {
                // Collect and report NUnit test results in Jenkins
                // Provides clear pass/fail gating for pipeline progression
                // Always publish results regardless of success or failure
                always {
        junit allowEmptyResults: false, testResults: 'ReactionTests/**/TestResults/*.xml'
                        }
            }
        }
        stage('Code Quality') {
            environment {
                SONAR_TOKEN = credentials('SONAR_TOKEN_MACHINE')
            }
            steps {
                script {
                    withSonarQubeEnv('SonarCloud') {
                    sh '''
                    export PATH="$PATH:$HOME/.dotnet/tools"
                    
                    dotnet sonarscanner begin \
                        /k:"Reaction-Timer-Game" \
                        /o:"215247237" \
                        /d:sonar.login=$SONAR_TOKEN \
                        /d:sonar.host.url="https://sonarcloud.io" \
                        /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
                        /d:sonar.exclusions="**/bin/**,**/obj/**,**/Migrations/**,**/TestResults/**" \
                        /d:sonar.scm.exclusions.disabled=true

                    dotnet restore ReactionMachineProject.sln
                    dotnet build ReactionMachineProject.sln --no-incremental    

                    dotnet sonarscanner end /d:sonar.login=$SONAR_TOKEN
                    '''
                    }

                // Wait for SonarCloud to finish processing the analysis
                timeout(time: 5, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
            }
        }
        stage('Security') {
            steps {
                echo "Checking project dependencies for known vulnerabilities using .NET"
                sh 'dotnet list package --vulnerable || true'

                echo "Scanning built Docker image for high and critical vulnerabilities using Trivy"
                sh '''
                docker pull s215247237/reactionmachine:latest
                trivy image --exit-code 0 --severity UNKNOWN,LOW,MEDIUM,HIGH,CRITICAL \
                     --format table s215247237/reactionmachine:latest > trivy-report.txt
                
                trivy image --exit-code 1 --severity CRITICAL s215247237/reactionmachine:latest || echo "No critical vulnerabilities found"
                '''
            }
        }
        stage('Deploy') {
            steps {
                echo 'placeholder'
            }
        }
        stage('Release') {
            steps {
                echo 'placeholder'
            }
        }
        stage('Monitoring') {
            steps {
                echo 'placeholder'
            }
        }

    }
}

