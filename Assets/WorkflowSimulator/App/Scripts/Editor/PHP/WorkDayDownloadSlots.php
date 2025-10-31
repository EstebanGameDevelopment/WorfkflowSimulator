<?php
	
	include 'ConfigurationWorkflowSimulator.php';
  
	$user_id = $_GET["user"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		DownloadSlots($user_id);
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
     //  DownloadSlots
     //-------------------------------------------------------------
     function DownloadSlots($user_par)
     {
		$query_consult = "SELECT * FROM projectslots WHERE user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::DownloadSlots::Select slots failed");

		$output_list = "";
		while ($row_data = mysqli_fetch_object($result_consult))
		{
			$slot_id = $row_data->id;
			$slot_project = $row_data->project;
			$slot_level = $row_data->level;
			$slot_timeout = $row_data->timeout;

			$entry_index = $slot_id . $GLOBALS['PARAM_SEPARATOR'] . $slot_project . $GLOBALS['PARAM_SEPARATOR'] . $slot_level . $GLOBALS['PARAM_SEPARATOR'] . $slot_timeout;
			$output_list = $output_list . $GLOBALS['LINE_SEPARATOR'] . $entry_index;
		}
		
		mysqli_free_result($result_consult);
		
		print "true" . $GLOBALS['BLOCK_SEPARATOR'] . $output_list;
    }	
	
?>
