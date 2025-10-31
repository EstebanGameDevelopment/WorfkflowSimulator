<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$id_user = $_POST["id"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($id_user, $passw_user, $salt_user))
	{
		$projectID = $_POST["project"];

		DeleteProjectData($id_user, $projectID);
		DeleteImageData($id_user, $projectID, -1);
		FreeProjectSlot($id_user, $projectID);
		
		print "true";
	}
	else
	{
		print "false";
	}		

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
	
?>
