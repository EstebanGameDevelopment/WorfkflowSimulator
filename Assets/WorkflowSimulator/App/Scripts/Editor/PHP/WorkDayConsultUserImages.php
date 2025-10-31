<?php
	
	include 'ConfigurationWorkflowSimulator.php';
 
	$user = $_GET["user"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];

	if (CheckValidUser($user, $passw_user, $salt_user))
	{
		ConsultUserImages($user);
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
     //  ConsultUserImages
     //-------------------------------------------------------------
     function ConsultUserImages($user_par)
     {
		$query_consult = "SELECT * FROM projectsimages WHERE user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::ConsultUserImages::Failed to get the images for user $user_par");

		$output_list = "";
		while ($row_data = mysqli_fetch_object($result_consult))
		{
			$id_image = $row_data->id;
			$name_image = $row_data->name;

			$entry_index = $id_image . $GLOBALS['PARAM_SEPARATOR'] . $name_image;
			$output_list = $output_list . $GLOBALS['LINE_SEPARATOR'] . $entry_index;
		}
		
		print "true" . $GLOBALS['BLOCK_SEPARATOR'] . $output_list;
		
		mysqli_free_result($result_consult);
    }	
	
?>
