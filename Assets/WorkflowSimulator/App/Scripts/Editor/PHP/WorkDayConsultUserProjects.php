<?php
	
	include 'ConfigurationWorkflowSimulator.php';
 
	$user = $_GET["user"];
	$passw_user = $_GET["password"];
	$salt_user = $_GET["salt"];

	if (CheckValidUser($user, $passw_user, $salt_user))
	{
		ConsultUserProjects($user);
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
     //  ConsultUserProjects
     //-------------------------------------------------------------
     function ConsultUserProjects($user_par)
     {
		$query_consult = "SELECT * FROM projectsindex WHERE user = $user_par";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'], $query_consult) or die("Query Error::ConsultUserProjects::Select stories index failed");

		$output_list = "";
		while ($row_bookindex = mysqli_fetch_object($result_consult))
		{
			$id_index = $row_bookindex->id;
			$data_index = $row_bookindex->data;
			$title_index = $row_bookindex->title;
			$description_index = $row_bookindex->description;
			$category1_index = $row_bookindex->category1;
			$category2_index = $row_bookindex->category2;
			$category3_index = $row_bookindex->category3;
			$time_index = $row_bookindex->time;

			$entry_index = $id_index . $GLOBALS['PARAM_SEPARATOR'] . $user_par . $GLOBALS['PARAM_SEPARATOR'] . $data_index . $GLOBALS['PARAM_SEPARATOR'] . $title_index . $GLOBALS['PARAM_SEPARATOR'] . $description_index . $GLOBALS['PARAM_SEPARATOR'] . $category1_index  . $GLOBALS['PARAM_SEPARATOR'] . $category2_index . $GLOBALS['PARAM_SEPARATOR'] . $category3_index . $GLOBALS['PARAM_SEPARATOR'] . $time_index;
			$output_list = $output_list . $GLOBALS['LINE_SEPARATOR'] . $entry_index;
		}
		
		print "true" . $GLOBALS['BLOCK_SEPARATOR'] . $output_list;
		
		mysqli_free_result($result_consult);
    }	
	
?>
