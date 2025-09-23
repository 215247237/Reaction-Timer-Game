pipeline {
    agent any
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        stage('Build') {
            steps {
                sh 'dotnet build ReactionMachineProject.sln'
            }
        }
        stage('Test') {
            steps {
                sh 'dotnet run --project Tester/tester.csproj'
            }
        }
    }
}

