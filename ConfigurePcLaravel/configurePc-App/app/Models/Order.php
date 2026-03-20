<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Order extends Model
{
      protected $fillable = [
        'order_date',
        'status',
        'customer_id',
    ];

    public function customer()
    {
        return $this->belongsTo(Customer::class, 'customer_id', 'id');
    }

    public function computer()
    {
        return $this->hasOne(Computer::class, 'order_id', 'id');
    }
}
