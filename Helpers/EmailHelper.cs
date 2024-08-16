using System;
using System.Net.Mail;

namespace Shoebill.Helpers;

public class EmailHelper
{
    public static bool IsEmailValid(string email)
    {
        try
        {
            var mail = new MailAddress(email);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
