<?php
	
	include 'ConfigurationUserManagement.php';
	
	$email = $_GET["email"];
	$password = $_GET["password"];

    // ++ LOGIN WITH email ++
	LoginEmail($email, $password);

    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);

?>
