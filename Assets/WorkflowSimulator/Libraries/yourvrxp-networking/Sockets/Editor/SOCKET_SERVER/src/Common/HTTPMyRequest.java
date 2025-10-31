package Common;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;

public class HTTPMyRequest {

	public static final String URL_ORGANIZE_CREATE 	= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameOrganizationCreate.php";
	public static final String URL_ORGANIZE_JOIN 	= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameOrganizationJoin.php";
	public static final String URL_ORGANIZE_DELETE 	= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameOrganizationDelete.php";
    
	public static final String URL_GAME_CREATE 		= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameRunningCreate.php";
    public static final String URL_GAME_JOIN 		= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameRunningJoin.php";
    public static final String URL_GAME_DELETE 		= "http://www.yourescaperoomvr.com/holodeck/php/server/ServerGameRunningDelete.php";

	
	public static String ExecutePost(String _targetURL, String _urlParameters) 
	{
		  /*
		  HttpURLConnection connection = null;

		  try {
		    //Create connection
		    URL url = new URL(_targetURL);
		    connection = (HttpURLConnection) url.openConnection();
		    connection.setRequestMethod("POST");
		    connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");

		    connection.setRequestProperty("Content-Length", Integer.toString(_urlParameters.getBytes().length));
		    connection.setRequestProperty("Content-Language", "en-US");  

		    connection.setUseCaches(false);
		    connection.setDoOutput(true);

		    //Send request
			// String urlParameters = "sn=C02G8416DRJM&cn=&locale=&caller=&num=12345";
		    DataOutputStream wr = new DataOutputStream (connection.getOutputStream());
		    wr.writeBytes(_urlParameters);
		    wr.close();

		    //Get Response  
		    InputStream is = connection.getInputStream();
		    BufferedReader rd = new BufferedReader(new InputStreamReader(is));
		    StringBuilder response = new StringBuilder(); // or StringBuffer if Java version 5+
		    String line;
		    while ((line = rd.readLine()) != null) {
		      response.append(line);
		      response.append('\r');
		    }
		    rd.close();
		    return response.toString();
		  } catch (Exception e) {
		    e.printStackTrace();
		    return null;
		  } finally {
		    if (connection != null) {
		      connection.disconnect();
		    }
		  }
		  */
		return "NOT ENABLED";
	}
}
