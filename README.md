Virtual printer support app full working Demo

Sample VS2022 (SDK>=10.0.26100.0) project covering capabilities documented at MSDN articles about "Print support app", specifically "Print Support App v4 API design guide".
The docs are missing reference to a full working demo, I struglled to make it work, so decided to share a working project. Valid as of 10/2025.
Most methods are empty and simply log. Some code added to ensure all methods are called.

Some gotchas that fail the app in weird or silent ways and would've been great if covered in the documentation:  
In Package.appxmanifest:  
= Printer name musty be specified in a resource file.  
= PreferredInputFormat="application/oxps" is required together with SupportedFormat Type="application/oxps".  
= Don't use backslashes in any strings (paths or names).  

In Tasks project (reference from App regardless if used there):  
= Must be Windows Runtime Component and target Windows SDK version >= 10.0.26100.0, same as the app.  
= The background task class must be public sealed and inherit from IBackgroundTask.  

If you create a msix package and deploy it manually you might need to add your certificate to Trusted Root(?!) and Trusted People.