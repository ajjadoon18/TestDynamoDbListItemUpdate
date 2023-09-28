# TestDynamoDbListItemUpdate

Just experimenting around if dynamo db can update specific properties of specific item in a list.

1. Identify role to use it would look something like this 
```
[profile prod-dynamodbcrud]
sso_start_url = https://mh-sso.awsapps.com/start
sso_region = eu-west-1
sso_account_id = 1234
sso_role_name = ProdDynamoDBCRUD
region = eu-west-1
```
2. Set variables accordingly in [Configuration.cs](https://github.com/ajjadoon18/DynamoDbDataExporter/blob/master/Configuration.cs)
3. Run command ``` aws sso login ```
4. Build and run
