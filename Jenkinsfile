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
                    COMMIT_HASH = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                }

                // Build Docker image and tag with both 'latest' and unique commit hash for traceability
                sh 'docker build -t reactionmachine:latest -t reactionmachine:${COMMIT_HASH} .'

                // Sign into Docker Hub using stored Jenkins credentials and push both tags
                withCredentials([usernamePassword(credentialsId: 'DOCKER_CREDENTIALS', usernameVariable: 'DOCKER_USERNAME', passwordVariable: 'DOCKER_PASSWORD')]) {
                    sh """
                        echo $DOCKER_PASSWORD | docker login -u $DOCKER_USERNAME --password-stdin
                        docker push reactionmachine:latest
                        docker push reactionmachine:${COMMIT_HASH}
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
                sh 'dotnet test ReactionTests/ReactionTests.csproj --no-build --logger "trx;LogFileName=nunit_test_results.trx"'

                // Collect and report NUnit test results in Jenkins
                // Provides clear pass/fail gating for pipeline progression
                junit 'ReactionTests/TestResults/nunit_test_results.trx'                             
            }
        }
        stage('Code Quality') {
            steps {

            }
        }
        stage('Security') {
            steps {

            }
        }
        stage('Deploy') {
            steps {

            }
        }
        stage('Release') {
            steps {

            }
        }
        stage('Monitoring') {
            steps {

            }
        }

    }
}

