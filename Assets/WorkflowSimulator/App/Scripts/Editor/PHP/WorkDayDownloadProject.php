<?php
	
	include 'ConfigurationWorkflowSimulator.php';
  
	$user_id = $_GET["user"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$id_story = $_GET["id"];

		if (ExistsProjectData($user_id, $id_story) == true)
		{
			DownloadProject($user_id, $id_story);
		}
		else
		{
			print "false";
		}
	}
	else
	{
		print "false";
	}		
	

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
	
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************
     // FUNCTIONS
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************   	
	
	 //-------------------------------------------------------------
     //  DownloadProject
     //-------------------------------------------------------------
     function DownloadProject($user_par, $id_par)
     {
		$query_consult = "SELECT * FROM projectsdata WHERE id = $id_par AND user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::UserConsult::Select POIs failed");

		if ($row_data = mysqli_fetch_object($result_consult))
		{
			$data = $row_data->data;

			print $data;
		}
		else
		{
			print "";
		}
		
		mysqli_free_result($result_consult);
    }	

	
?>
