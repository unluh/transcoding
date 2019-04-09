#rights=ADMIN
//------------------------------------------------------------------- 
// ==ServerScript==
// @name            Concatenation
// @status on
// @description     
// @include        .*
// @exclude        
// @responsecode    200
// ==/ServerScript==
// --------------------------------------------------------------------
// Note: use httpMessage object methods to manipulate HTTP Message
// use debug(String s) method to trace items in service log (with log level >=FINE)
// ---------------
import java.util.UUID;
import java.io.*;
import java.io.File; 
import java.io.FileNotFoundException; 
import java.util.Scanner; 
public void main(HttpMessage httpMessage){
    //start your code from here
    UUID id= UUID.randomUUID();
    String fileName="/tmp/"+id+".html";
    File file = new File(fileName);
    
    try{
        System.out.println("Process is starting");
        System.out.println(httpMessage.getUrl());
        
        PrintWriter writer = new PrintWriter(fileName, "UTF-8");
        writer.write(httpMessage.getBody());
        writer.close();

        System.out.println(fileName);
        ProcessBuilder pb2 = new ProcessBuilder("/home/pcroot/dotnet/dotnet","/home/pcroot/Desktop/ProxyApp/Proxy.ConsoleApp/bin/Debug/netcoreapp2.1/Proxy.ConsoleApp.dll","-u",httpMessage.getUrl(),"-f",fileName,"-css","-js","-mcss","-mjs");
        final Process process = pb2.start();

        int errCode = process.waitFor();
        System.out.println(httpMessage.getUrl()+ " "+errCode);
        
        Scanner sc = new Scanner(file); 
        sc.useDelimiter("\\Z"); 
        String body= sc.next();


        System.out.println(body);
        httpMessage.setBody(body);
    }catch(Exception ex) {
        System.out.println(httpMessage.getUrl()+ " Error Occured while running script\n"+ex.getMessage());
    }finally {  
        file.delete();
    }
}
