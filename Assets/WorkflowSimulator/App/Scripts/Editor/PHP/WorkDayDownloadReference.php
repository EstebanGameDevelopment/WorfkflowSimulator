<?php
	
	include 'ConfigurationWorkflowSimulator.php';
  
	$user_id = $_GET["user"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$size_project = $_GET["size"];

		DownloadReference($size_project);
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
     //  DownloadReference
     //-------------------------------------------------------------
     function DownloadReference($size_project_par)
     {
		$query_consult = "SELECT * FROM projectsreference WHERE completed = 0 ORDER BY size";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::DownloadReference::List references failed");

		$size_previous = 0;
		$data = '';
		
		while ($row_data = mysqli_fetch_object($result_consult))
		{
			$size_reference = $row_data->size;
			
			if (($size_previous <= $size_project_par) && ($size_project_par <= $size_reference))
			{				
				$data = $row_data->data;
			}
			$size_previous = $size_reference;
		}
		
		print $data;
		
		mysqli_free_result($result_consult);
    }	

	
?>
