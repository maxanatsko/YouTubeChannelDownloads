import fabric.functions as fn

udf = fn.UserDataFunctions()

@udf.connection(argName="sqlDB",alias="YT")
@udf.function() 

# Take a product description and product model ID as input parameters and write them back to the SQL database
# Users will provide these parameters in the PowerBI report
def write_one_to_sql_db(sqlDB: fn.FabricSqlConnection, productDescription: str, productModelId:int) -> str: 

    # Error handling to ensure product description doesn't go above 200 characters
    if(len(productDescription) > 200):
        raise fn.UserThrownError("Descriptions have a 200 character limit. Please shorten your description.", {"Description:": productDescription})

    # Establish a connection to the SQL database  
    connection = sqlDB.connect() 
    cursor = connection.cursor() 

    # Insert data into the ProductDescription table  
    insert_description_query = "INSERT INTO [SalesLT].[ProductDescription] (Description) OUTPUT INSERTED.ProductDescriptionID VALUES (?)" 
    cursor.execute(insert_description_query, productDescription) 

    # Get the result from the previous query 
    results = cursor.fetchall() 

    # Insert data into the ProductModelProductDescription table 
    insert_model_description_query = "INSERT INTO [SalesLT].[ProductModelProductDescription] (ProductModelID, ProductDescriptionID, Culture) VALUES (?, ?, ?);" 
    cursor.execute(insert_model_description_query, (productModelId, results[0][0], "en-US")) 

    # Commit the transaction 
    connection.commit() 
    cursor.close() 
    connection.close()  

    return "Product description was added"
