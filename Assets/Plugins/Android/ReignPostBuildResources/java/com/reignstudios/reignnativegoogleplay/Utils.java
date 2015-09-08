package com.reignstudios.reignnativegoogleplay;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import android.util.Log;

public class Utils
{
	public static final String MD5(final String s)
	{
	    try
	    {
	        // Create MD5 Hash
	        MessageDigest digest = java.security.MessageDigest.getInstance("MD5");
	        digest.update(s.getBytes());
	        byte messageDigest[] = digest.digest();

	        // Create Hex String
	        StringBuffer hexString = new StringBuffer();
	        for (int i = 0; i < messageDigest.length; i++)
	        {
	            String h = Integer.toHexString(0xFF & messageDigest[i]);
	            while (h.length() < 2)
	            {
	                h = "0" + h;
	            }
	            
	            hexString.append(h);
	        }
	        
	        return hexString.toString();

	    }
	    catch (NoSuchAlgorithmException e)
	    {
	        Log.d("md5", e.getMessage());
	    }
	    
	    return "";
	}
}
