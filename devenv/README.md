# devenv

The development environment provides tooling to help with tasks in the dev workflow.

The devenv uses the same Kubernetes Cluster as the Dev Application.

Start the devenv by running `plz run devenv`, which runs the dev tooling in the dev namespace in the cluster with helm.

## SonarQube

SonarQube can be used to run Code Scans. 

### Code Scanning

The following steps can be followed to run code scans. This needs to be done manually at the time of writing, but can be automated later.

- Navigate to the SonarQube UI at http://localhost:31000
- Update the default admin credentials. Credentials are `admin` and `admin` for username and password when the SonarQube instance first runs.
- Create a Global Token `code-scan-key` at http://localhost:31000/account/security
- Create a new project http://localhost:31000/projects/create?mode=manual and name it with the service to be scanned.
- You can run code scans now with this command. 

```
# change the project name
kubectl exec -n payobills-dev deploy/sonar-scanner-cli -- bash -c 'sonar-scanner \
  -Dsonar.projectKey=ui \
  -Dsonar.sources='/app/ui/src' \
  -Dsonar.host.url=http://sonarqube-sonarqube:9000 \
  -Dsonar.login=<code-scan-key>
'
```

You'll be able to see the report in the SonarQube UI at http://localhost:31000/projects
