<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$id_user = $_POST["id"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($id_user, $passw_user, $salt_user))
	{
		DeleteProjectData($id_user, -1);
		DeleteImageData($id_user, -1, -1);
		DeleteProjectSlotsForUser($id_user);
		
		print "true";
	}
	else
	{
		print "false";
	}		

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
	
?>
