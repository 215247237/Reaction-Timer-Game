pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                // Pull the project from the GitHub repo
                checkout scm

                // Build the Reaction Machine Game by compiling the sln file
                sh 'dotnet build ReactionMachineProject.sln'
                
                // Create a Docker image as a build artefact
                sh 'docker build -t reactionmachine .'

                // Show build artefact
                sh 'docker images | grep reactionmachine'
            }
        }
        stage('Test') {
            steps {
                // Run the Tester project (EnhancedTester.cs) to execute all tests
                sh 'dotnet run --project Tester/tester.csproj'
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

