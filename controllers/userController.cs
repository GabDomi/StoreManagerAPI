namespace loja.controllers;
public class UserController{
    public bool login(string user, string password){
        bool response = false;
        if(user == "admin" && password == "abc"){
            response = true;
        }
        return response;
    }
}