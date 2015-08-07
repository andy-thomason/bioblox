<?php
$db = mysql_connect('igor.gold.ac.uk', 'acurt001', 'MrGene') or die('Could not connect: ' . mysql_error()); 
mysql_select_db('acurt001') or die('Could not select database');

$level = mysql_real_escape_string($_GET['level'],$db);

$hash = mysql_real_escape_string($_GET['hash'],$db);

$secretKey = "plznohackkthx";

$realHash = md5($level . $secretKey);

$date = date('Y-m-d 0:0:0');
$endOfDay =  date('Y-m-d 23:59:59');

if($realHash == $hash)
{
	$query = "SELECT * FROM BioBlox WHERE level = '$level' AND date between '$date' and '$endOfDay' ORDER BY DESC score 
LIMIT 5;";

	$result = mysql_query($query) or die ('Query failed: ' . mysql_error());
	while($row = mysql_fetch_object($result))
	{
		echo $row ->score . ',';
	}
}

?>