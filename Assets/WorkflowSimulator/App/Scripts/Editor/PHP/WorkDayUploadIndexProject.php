<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$user_id = $_POST["usr"];
	$passw_user = $_POST["pwd"];
	$salt_user = $_POST["salt"];

	if (CheckValidUser($user_id, $passw_user, $salt_user))
	{
		$id = $_POST["id"];
		$user = $_POST["user"];
		$dataid = $_POST["dataid"];
		$title = $_POST["title"];
		$description = $_POST["description"];
		$category1 = $_POST["category1"];
		$category2 = $_POST["category2"];
		$category3 = $_POST["category3"];
		$timestamp = $_POST["time"];

		UploadStoryIndex($id, $user, $dataid, $title, $description, $category1, $category2, $category3, $timestamp, true);
	}
	else
	{
		print "false";
	}		


    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
?>
