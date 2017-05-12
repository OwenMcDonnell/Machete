declare @startDate DateTime = '1/1/2016'
declare @endDate DateTime = '1/1/2017'

select 
convert(varchar(24), @startDate, 126) + '-' + convert(varchar(23), @endDate, 126) as id,
homeless as label, 
count(*) as value
FROM (
  select W.ID, 
  CASE 
	WHEN W.livewithchildren = 1 then 'Households with minors - details unknown'
	when W.livealone = 1 then 'Single person household - details unknown'
	when W.livealone = 0 and W.livewithchildren = 0 then 'Household composition unknown'
  END as homeless
  from Workers W
  JOIN dbo.WorkerSignins WSI ON W.ID = WSI.WorkerID
  WHERE dateforsignin >= @startDate and dateforsignin <= @endDate
  group by W.ID, W.homeless
) as WW
group by livewithcildren, livealone