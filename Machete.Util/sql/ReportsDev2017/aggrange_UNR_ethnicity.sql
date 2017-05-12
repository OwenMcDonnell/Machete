declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-' + min(raceID) as id,
raceID as label, 
count(*) as value
FROM (
  select W.ID, 
  CASE 
	WHEN W.raceID = 5 then 'Spanish/Hispanic/Latino'
	when W.raceID <> 5 then 'Not Spanish/Hispanic/Latino'
	when W.raceID is null then 'NULL'
  END as raceID
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.raceID
) as WW
group by raceID