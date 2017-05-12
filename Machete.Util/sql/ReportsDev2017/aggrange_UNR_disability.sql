declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) + '-' + min(disabled) as id,
disabled as label, 
count(*) as value
FROM (
  select W.ID, 
  CASE 
	WHEN W.disabled = 1 then 'yes'
	when W.disabled = 0 then 'no'
	when W.disabled is null then 'NULL'
  END as disabled
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.disabled
) as WW
group by disabled