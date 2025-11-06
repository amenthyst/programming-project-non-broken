using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class MenuInputManager : Node
{
    private string username = "";
    private string password = "";
    private Label loginMessage;

    private Dictionary<string, string> credentialDict = new Dictionary<string, string>();
    private void load_credentials()
    {
        StreamReader sr = new StreamReader("/Users/amenthyst/Godot/programming-project/scripts/logindata.txt");
        string line;
        while (!sr.EndOfStream)
        {
            line = sr.ReadLine();
            string[] record = line.Split('/');
            credentialDict.Add(record[0], record[1]);
        }
        sr.Close();
    }
    public override void _Ready()
    {
        load_credentials();
        loginMessage = GetNode<Label>("../VBoxContainer/HBoxContainer/LoginMessage");
    }
    
    private void _on_username_input_box_text_changed(string text)
    {
        username = text;
    }
    private void _on_password_input_box_text_changed(string text)
    {
        password = text;
    }
    private void _on_submit_button_pressed()
    {
        loginMessage.Text = validate(username, password);
    }
    private string validate(string username, string password)
    {
        if (username == "" || password == "")
        {
            return "One or more fields empty";
        }
        if (username.Length < 5 || password.Length < 5)
        {
            return "Username or password too short";
        }
        foreach (char c in username)
        {
            if (c == ' ')
            {
                return "Invalid character found in username";
            }
        }
        foreach (char c in password)
        {
            if (c == ' ')
            {
                return "Invalid character found in password";
            }
        }
        if (!credentialDict.ContainsKey(username))
        {
            return "Username not recognized";
        }
        if (password != credentialDict[username])
        {
            return "Password incorrect";
        }
        return "Login successful";
    }
}
