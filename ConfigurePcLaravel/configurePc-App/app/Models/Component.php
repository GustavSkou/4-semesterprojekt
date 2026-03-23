<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Component extends Model
{
    protected $fillable = [
        'name',
        'brand',
        'tray_id',
        'category_id',
        'price',
    ];

    public function category()
    {
        return $this->belongsTo(Category::class, 'category_id', 'id');
    }

    public function computers()
    {
        return $this->belongsToMany(Computer::class, 'computer_component_list');
    }

    public function specifications()
    {
        return $this->hasMany(Specification::class);
    }

    public function wattageLists()
    {
        return $this->hasMany(WattageList::class);
    }

    public function requirements()
    {
        return $this->hasMany(Requirement::class);
    }
}
