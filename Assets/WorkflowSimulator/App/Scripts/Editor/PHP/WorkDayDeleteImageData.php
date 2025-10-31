<?php
	
	include 'ConfigurationWorkflowSimulator.php';

	$id_user = $_POST["id"];
	$passw_user = $_POST["password"];
	$salt_user = $_POST["salt"];


	if (CheckValidUser($id_user, $passw_user, $salt_user))
	{
		$data = $_POST["data"];

		$item_image = explode(';', $data);
		$number_images = count($item_image);

		$i = 0;
		for ($i = 0; $i < $number_images; $i++)			 
		{
			DeleteImageData($id_user, -1, intval($item_image[$i]));
		}
		
		print "true";
	}
	else
	{
		print "false";
	}		

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);

?>
