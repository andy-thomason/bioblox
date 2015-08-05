<?php
 $db = mysql_connect('igor.gold.ac.uk', 'acurt001', 'MrGene') or die('Could not connect: ' . mysql_error()); 
mysql_select_db('acurt001') or die('Could not select database');


$level = mysql_real_escape_string($_GET['level'],$db);
$name = mysql_real_escape_string($_GET['name'],$db);
$postcode = mysql_real_escape_string($_GET['post'],$db);
$score = $_GET['score'];

$hash = $_GET['hash'];

$secretKey = "plznohackkthx";


$realHash = md5($level . $name . $postcode . $score . $secretKey);

$dateTime = date('Y-m-d H:i:s');

if($realHash == $hash)
{
	$query = "INSERT INTO BioBlox VALUES ('$level','$name','$postcode','$score',NULL,'$dateTime');";

	$result = mysql_query($query) or die ('Query failed: ' . mysql_error());
} else {
echo("security error");
}

?>