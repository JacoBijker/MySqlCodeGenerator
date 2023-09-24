# MySql Code Generator
This tool was originally created to remove the monotonous task of creating CRUD stored procedures for MySql Databases.

The tool generates the following stored procedures:
- InsUpd (For inserting and updating records)
- GetById (Fetching by Primary Key)
- GetByUniqueId (Fetching by Unique Index)
- GetByIds (Fetching by Foreign keys)
- GetPagedSearch (Fetching with paging parameters)

This tool also creates the corresponding Repository and Domain layers as partial C# classes, facilitating straightforward extensibility.
